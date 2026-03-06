using System.ComponentModel;
using System.Text.Json;
using companyOSINT.Application.Interfaces;
using ModelContextProtocol.Server;

namespace companyOSINT.Web.McpTools;

[McpServerToolType]
public sealed class ListToolsTool(ICompanyService companyService)
{
    [McpServerTool(Name = "list_tools"), Description(
        "List all detected web tool names (e.g. Google Analytics, Cloudflare, Matomo). " +
        "Use these names as filters in the search_companies tool's toolNames parameter.")]
    public async Task<string> ListTools(CancellationToken cancellationToken = default)
    {
        var names = await companyService.GetDistinctToolNamesAsync(cancellationToken);
        return JsonSerializer.Serialize(names, new JsonSerializerOptions { WriteIndented = true });
    }
}
