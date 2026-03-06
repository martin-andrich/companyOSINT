using companyOSINT.Domain.Dtos.Websites;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace companyOSINT.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebsitesController(IWebsiteService websiteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Website>>> GetByCompany([FromQuery] Guid companyId)
    {
        return await websiteService.GetByCompanyAsync(companyId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Website>> Get(Guid id)
    {
        var website = await websiteService.GetByIdAsync(id);

        if (website is null)
            return NotFound();

        return website;
    }

    [HttpPost]
    public async Task<ActionResult<Website>> Create(WebsiteCreateDto dto)
    {
        var (website, error) = await websiteService.CreateAsync(dto);

        if (error is not null)
            return NotFound(error);

        return CreatedAtAction(nameof(Get), new { id = website!.Id }, website);
    }

    [HttpGet("next-to-enrich")]
    public async Task<IActionResult> GetNextToEnrich()
    {
        var website = await websiteService.GetNextToEnrichAsync();

        if (website is null)
            return NoContent();

        return Ok(website);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, WebsitePatchDto dto)
    {
        if (!await websiteService.PatchAsync(id, dto))
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await websiteService.DeleteAsync(id))
            return NotFound();

        return NoContent();
    }

    [HttpGet("{websiteId:guid}/software")]
    public async Task<ActionResult<List<Software>>> GetSoftware(Guid websiteId)
    {
        if (await websiteService.GetByIdAsync(websiteId) is null)
            return NotFound("Website not found");

        return Ok(await websiteService.GetSoftwareAsync(websiteId));
    }

    [HttpPut("{websiteId:guid}/software")]
    public async Task<ActionResult<List<Software>>> ReplaceSoftware(Guid websiteId, List<SoftwareCreateDto> dtos)
    {
        var (software, error) = await websiteService.ReplaceSoftwareAsync(websiteId, dtos);

        if (error is not null)
            return NotFound(error);

        return Ok(software);
    }

    [HttpGet("{websiteId:guid}/tools")]
    public async Task<ActionResult<List<Tool>>> GetTools(Guid websiteId)
    {
        if (await websiteService.GetByIdAsync(websiteId) is null)
            return NotFound("Website not found");

        return Ok(await websiteService.GetToolsAsync(websiteId));
    }

    [HttpPut("{websiteId:guid}/tools")]
    public async Task<ActionResult<List<Tool>>> ReplaceTools(Guid websiteId, List<ToolCreateDto> dtos)
    {
        var (tools, error) = await websiteService.ReplaceToolsAsync(websiteId, dtos);

        if (error is not null)
            return NotFound(error);

        return Ok(tools);
    }
}
