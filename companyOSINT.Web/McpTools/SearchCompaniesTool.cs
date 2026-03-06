using System.ComponentModel;
using System.Text.Json;
using companyOSINT.Application.Interfaces;
using ModelContextProtocol.Server;

namespace companyOSINT.Web.McpTools;

[McpServerToolType]
public sealed class SearchCompaniesTool(ICompanyService companyService)
{
    [McpServerTool(Name = "search_companies"), Description(
        "Search for German companies by location, industry sector, technology stack, and website quality. " +
        "Returns paginated results. Use list_sectors, list_software, and list_tools first to get valid filter values.")]
    public async Task<string> SearchCompanies(
        [Description("German postal code (PLZ) to search around, e.g. '01067' for Dresden")] string? postalCode = null,
        [Description("Search radius in kilometers around the postal code (default: 50)")] int radiusKm = 50,
        [Description("List of sector IDs to filter by (use list_sectors to get IDs)")] List<Guid>? sectorIds = null,
        [Description("List of software/CMS names to filter by, e.g. ['WordPress', 'Shopware']")] List<string>? softwareNames = null,
        [Description("List of tool names to filter by, e.g. ['Google Analytics', 'Matomo']")] List<string>? toolNames = null,
        [Description("Filter by SSL certificate validity")] bool? sslValid = null,
        [Description("Filter by consent manager presence")] bool? consentManagerFound = null,
        [Description("Filter by whether company has a website")] bool? hasWebsite = null,
        [Description("Filter by time-to-first-byte: 'fast' (<=800ms), 'medium' (800-1800ms), 'slow' (>1800ms)")] string? ttfbCategory = null,
        [Description("Filter by whether website makes requests without consent")] bool? hasRequestsWithoutConsent = null,
        [Description("Page number (default: 1)")] int page = 1,
        [Description("Results per page, max 100 (default: 25)")] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await companyService.SearchAsync(
            postalCode, radiusKm, sectorIds, softwareNames, toolNames,
            sslValid, consentManagerFound, hasWebsite, ttfbCategory,
            hasRequestsWithoutConsent, page, pageSize, cancellationToken);

        return JsonSerializer.Serialize(new
        {
            result.TotalCount,
            result.Page,
            result.PageSize,
            TotalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize),
            Companies = result.Items.Select(c => new
            {
                c.Id,
                c.Name,
                c.RegisteredAddress,
                c.Website,
                c.Activity,
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
