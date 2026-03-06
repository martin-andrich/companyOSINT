using System.ComponentModel;
using System.Text.Json;
using companyOSINT.Application.Interfaces;
using ModelContextProtocol.Server;

namespace companyOSINT.Web.McpTools;

[McpServerToolType]
public sealed class ListSoftwareTool(ICompanyService companyService)
{
    [McpServerTool(Name = "list_software"), Description(
        "List all detected software and CMS names (e.g. WordPress, Shopware, TYPO3). " +
        "Use these names as filters in the search_companies tool's softwareNames parameter.")]
    public async Task<string> ListSoftware(CancellationToken cancellationToken = default)
    {
        var names = await companyService.GetDistinctSoftwareNamesAsync(cancellationToken);
        return JsonSerializer.Serialize(names, new JsonSerializerOptions { WriteIndented = true });
    }
}
