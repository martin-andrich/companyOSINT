using System.ComponentModel;
using System.Text.Json;
using companyOSINT.Application.Interfaces;
using ModelContextProtocol.Server;

namespace companyOSINT.Web.McpTools;

[McpServerToolType]
public sealed class GetCompanyDetailsTool(ICompanyService companyService)
{
    [McpServerTool(Name = "get_company_details"), Description(
        "Get full details for a specific company including contacts, websites, detected software, and tools.")]
    public async Task<string> GetCompanyDetails(
        [Description("The company ID (GUID) to retrieve")] Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var company = await companyService.GetByIdAsync(companyId, cancellationToken);

        if (company is null)
            return JsonSerializer.Serialize(new { error = "Company not found" });

        return JsonSerializer.Serialize(new
        {
            company.Id,
            company.Name,
            company.RegisteredAddress,
            company.PostalCode,
            company.FederalState,
            company.RegisteredOffice,
            company.Registrar,
            company.RegisterArt,
            company.RegisterNummer,
            Sector = company.Sector?.Name,
            company.Activity,
            Contacts = company.Contacts.Select(c => new
            {
                c.Name,
                c.FirstName,
                c.LastName,
                c.Title,
                c.Position,
                c.Type,
                c.City,
            }),
            Websites = company.Websites.Select(w => new
            {
                w.Id,
                w.UrlWebsite,
                w.UrlImprint,
                w.HttpResponseCode,
                w.SslValid,
                w.ConsentManagerFound,
                w.RequestsWithoutConsent,
                w.CookiesWithoutConsent,
                w.AverageTimeToFirstByte,
                Software = w.Softwares.Select(s => new { s.Name, s.Version }),
                Tools = w.Tools.Select(t => new { t.Name }),
            }),
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
