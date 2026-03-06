using System.Globalization;
using System.Reflection;
using companyOSINT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Infrastructure.Data;

public static class PostalCodeSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.PostalCodes.AnyAsync())
            return;

        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "companyOSINT.Infrastructure.Data.SeedData.postal_codes_de.csv";

        await using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);

        var postalCodes = new List<PostalCode>();

        while (await reader.ReadLineAsync() is { } line)
        {
            var parts = line.Split(';');
            if (parts.Length < 4) continue;

            postalCodes.Add(new PostalCode
            {
                Code = parts[0],
                Place = parts[1],
                Latitude = double.Parse(parts[2], CultureInfo.InvariantCulture),
                Longitude = double.Parse(parts[3], CultureInfo.InvariantCulture),
            });
        }

        db.PostalCodes.AddRange(postalCodes);
        await db.SaveChangesAsync();
    }
}
