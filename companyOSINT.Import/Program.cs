using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Npgsql;
using NpgsqlTypes;
using companyOSINT.Domain.Entities;

var sqlitePath = args.Length > 0 ? args[0] : "handelsregister.db";
var pgConnectionString = args.Length > 1
    ? args[1]
    : "Host=localhost;Database=companyOSINT;Username=postgres;Password=postgres";

const int batchSize = 50_000;

Console.WriteLine($"SQLite:     {sqlitePath}");
Console.WriteLine($"PostgreSQL: {pgConnectionString}");
Console.WriteLine();

// Schema is managed via EF Core migrations in companyOSINT.Web.
// Run: dotnet ef database update --project companyOSINT.Web --startup-project companyOSINT.Web

// ── Open connections ────────────────────────────────────────────────────────

using var sqlite = new SqliteConnection($"Data Source={sqlitePath};Mode=ReadOnly");
await sqlite.OpenAsync();

await using var pg = new NpgsqlConnection(pgConnectionString);
await pg.OpenAsync();

var sw = Stopwatch.StartNew();

var totalCompanies = Convert.ToInt64(
    new SqliteCommand("SELECT COUNT(*) FROM company WHERE current_status = 'currently registered'", sqlite).ExecuteScalar());
Console.WriteLine($"Source: {totalCompanies:N0} companies (currently registered)\n");

// ── Read companies + contacts via LEFT JOIN ─────────────────────────────────
//
//  Columns 0-7:   company fields
//  Columns 8-21:  officer fields (NULL when company has no officers)
//
using var reader = new SqliteCommand("""
    SELECT c.id, c.name, c.registered_address, c.federal_state,
           c.registered_office, c.registrar, c.register_art, c.register_nummer,
           o.id, o.name, o.firstname, o.lastname, o.maidenname, o.title,
           o.position, o.type, o.city, o.flag,
           o.start_date, o.end_date, o.dismissed, o.reference_no
    FROM company c
    LEFT JOIN officer o ON c.company_number = o.company_id
    WHERE c.current_status = 'currently registered'
    ORDER BY c.id
    """, sqlite).ExecuteReader();

var companyIdMap = new Dictionary<int, Guid>();
var seenContactIds = new HashSet<int>();
long importedCompanies = 0;
long importedContacts = 0;

var companyBatch = new List<Company>();
var contactBatch = new List<Contact>();

while (reader.Read())
{
    var companyIntId = reader.GetInt32(0);

    // Deduplicate companies (LEFT JOIN repeats company data per officer)
    if (!companyIdMap.ContainsKey(companyIntId))
    {
        var companyGuid = Guid.NewGuid();
        companyIdMap[companyIntId] = companyGuid;
        var rawAddress = Str(reader, 2);
        var address = CleanAddress(rawAddress);
        companyBatch.Add(new Company
        {
            Id = companyGuid,
            Name = Str(reader, 1),
            RegisteredAddress = address,
            PostalCode = ExtractPostalCode(address),
            FederalState = Str(reader, 3),
            RegisteredOffice = Str(reader, 4),
            Registrar = Str(reader, 5),
            RegisterArt = Str(reader, 6),
            RegisterNummer = Str(reader, 7),
        });
    }

    // Add contact if officer row is present (not NULL from LEFT JOIN)
    if (!reader.IsDBNull(8) && seenContactIds.Add(reader.GetInt32(8)))
    {
        contactBatch.Add(new Contact
        {
            Id = Guid.NewGuid(),
            Name = Str(reader, 9),
            FirstName = Str(reader, 10),
            LastName = Str(reader, 11),
            MaidenName = Str(reader, 12),
            Title = Str(reader, 13),
            Position = Str(reader, 14),
            Type = Str(reader, 15),
            City = Str(reader, 16),
            Flag = Str(reader, 17),
            CompanyId = companyIdMap[companyIntId],
            StartDate = Str(reader, 18),
            EndDate = Str(reader, 19),
            Dismissed = Str(reader, 20),
            ReferenceNo = Str(reader, 21),
        });
    }

    // Flush companies when batch is full
    if (companyBatch.Count >= batchSize)
    {
        await FlushCompaniesAsync(pg, companyBatch);
        importedCompanies += companyBatch.Count;
        companyBatch.Clear();
        Console.Write($"\r  Companies: {importedCompanies:N0} / {totalCompanies:N0}" +
                      $"  Contacts: {importedContacts:N0}");
    }

    // Flush contacts when batch is full (always flush pending companies first for FK)
    if (contactBatch.Count >= batchSize)
    {
        if (companyBatch.Count > 0)
        {
            await FlushCompaniesAsync(pg, companyBatch);
            importedCompanies += companyBatch.Count;
            companyBatch.Clear();
        }

        await FlushContactsAsync(pg, contactBatch);
        importedContacts += contactBatch.Count;
        contactBatch.Clear();
        Console.Write($"\r  Companies: {importedCompanies:N0} / {totalCompanies:N0}" +
                      $"  Contacts: {importedContacts:N0}");
    }
}

// Flush remaining (companies before contacts for FK integrity)
if (companyBatch.Count > 0)
{
    await FlushCompaniesAsync(pg, companyBatch);
    importedCompanies += companyBatch.Count;
}

if (contactBatch.Count > 0)
{
    await FlushContactsAsync(pg, contactBatch);
    importedContacts += contactBatch.Count;
}

Console.WriteLine($"\r  Companies: {importedCompanies:N0}  Contacts: {importedContacts:N0}          ");
Console.WriteLine($"\nDone in {sw.Elapsed:hh\\:mm\\:ss}.");

// ── SQLite read helpers ─────────────────────────────────────────────────────

static string? Str(SqliteDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);

static string? ExtractPostalCode(string? address)
{
    if (address is null) return null;
    var match = Regex.Match(address, @",\s*(\d{5})\s");
    return match.Success ? match.Groups[1].Value : null;
}

static string? CleanAddress(string? address)
{
    if (string.IsNullOrWhiteSpace(address)) return address;

    // Keywords that signal the end of the actual address in the Handelsregister data.
    // Everything from one of these keywords onwards is legal/corporate text, not address.
    ReadOnlySpan<string> keywords =
    [
        "Rechtsform:", "Gegenstand:", "Inhaber:", "Stamm- bzw.", "Kapital:",
        "Vertretungsregelung:", "Prokura:", "Ausgeschieden:", "Nicht mehr",
        "Geschäftsführer:", "Liquidator:", "Alleinvertretungsbefugnis",
        "Einzelprokura", "Gesamtprokura", "Persönlich haftender",
    ];

    // Find the earliest keyword occurrence and cut there
    var cutIndex = address.Length;
    foreach (var keyword in keywords)
    {
        var idx = address.IndexOf(keyword, StringComparison.Ordinal);
        if (idx >= 0 && idx < cutIndex)
            cutIndex = idx;
    }

    var cleaned = address[..cutIndex].TrimEnd(' ', '.', ',');

    // If there was no keyword but the address contains a PLZ pattern followed by
    // a place name and then a period, trim the trailing period
    // (many clean addresses end with "01067 Dresden.")
    return cleaned.Length > 0 ? cleaned : null;
}

// ── PostgreSQL helpers ──────────────────────────────────────────────────────

static async Task WriteText(NpgsqlBinaryImporter w, string? value)
{
    if (value is null)
        await w.WriteNullAsync();
    else
        await w.WriteAsync(value, NpgsqlDbType.Text);
}

// ── Batch flush: Companies ──────────────────────────────────────────────────

static async Task FlushCompaniesAsync(NpgsqlConnection pg, List<Company> batch)
{
    var now = DateTime.UtcNow;

    await using var writer = await pg.BeginBinaryImportAsync("""
        COPY "Companies" (
            "Id","Name","RegisteredAddress","PostalCode","FederalState",
            "RegisteredOffice","Registrar","RegisterArt","RegisterNummer",
            "DateCreated","DateModified"
        ) FROM STDIN (FORMAT BINARY)
        """);

    foreach (var c in batch)
    {
        await writer.StartRowAsync();
        await writer.WriteAsync(c.Id, NpgsqlDbType.Uuid);
        await WriteText(writer, c.Name);
        await WriteText(writer, c.RegisteredAddress);
        await WriteText(writer, c.PostalCode);
        await WriteText(writer, c.FederalState);
        await WriteText(writer, c.RegisteredOffice);
        await WriteText(writer, c.Registrar);
        await WriteText(writer, c.RegisterArt);
        await WriteText(writer, c.RegisterNummer);
        await writer.WriteAsync(now, NpgsqlDbType.TimestampTz);
        await writer.WriteAsync(now, NpgsqlDbType.TimestampTz);
    }

    await writer.CompleteAsync();
}

// ── Batch flush: Contacts ───────────────────────────────────────────────────

static async Task FlushContactsAsync(NpgsqlConnection pg, List<Contact> batch)
{
    var now = DateTime.UtcNow;

    await using var writer = await pg.BeginBinaryImportAsync("""
        COPY "Contacts" (
            "Id","Name","FirstName","LastName","MaidenName","Title",
            "Position","Type","City","Flag","CompanyId",
            "StartDate","EndDate","Dismissed","ReferenceNo",
            "DateCreated","DateModified"
        ) FROM STDIN (FORMAT BINARY)
        """);

    foreach (var c in batch)
    {
        await writer.StartRowAsync();
        await writer.WriteAsync(c.Id, NpgsqlDbType.Uuid);
        await WriteText(writer, c.Name);
        await WriteText(writer, c.FirstName);
        await WriteText(writer, c.LastName);
        await WriteText(writer, c.MaidenName);
        await WriteText(writer, c.Title);
        await WriteText(writer, c.Position);
        await WriteText(writer, c.Type);
        await WriteText(writer, c.City);
        await WriteText(writer, c.Flag);
        await writer.WriteAsync(c.CompanyId, NpgsqlDbType.Uuid);
        await WriteText(writer, c.StartDate);
        await WriteText(writer, c.EndDate);
        await WriteText(writer, c.Dismissed);
        await WriteText(writer, c.ReferenceNo);
        await writer.WriteAsync(now, NpgsqlDbType.TimestampTz);
        await writer.WriteAsync(now, NpgsqlDbType.TimestampTz);
    }

    await writer.CompleteAsync();
}

