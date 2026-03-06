using System.ComponentModel;
using System.Text.Json;
using companyOSINT.Application.Interfaces;
using ModelContextProtocol.Server;

namespace companyOSINT.Web.McpTools;

[McpServerToolType]
public sealed class ListSectorsTool(ISectorService sectorService)
{
    [McpServerTool(Name = "list_sectors"), Description(
        "List all available industry sectors with their IDs and German names. " +
        "Use sector IDs as filters in the search_companies tool.")]
    public async Task<string> ListSectors(CancellationToken cancellationToken = default)
    {
        var sectors = await sectorService.GetAllAsync(cancellationToken);

        return JsonSerializer.Serialize(
            sectors.Select(s => new { s.Id, s.Name }),
            new JsonSerializerOptions { WriteIndented = true });
    }
}
