using companyOSINT.Domain.Dtos.Sectors;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace companyOSINT.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectorsController(ISectorService sectorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Sector>>> GetAll()
    {
        return await sectorService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Sector>> Get(Guid id)
    {
        var sector = await sectorService.GetByIdAsync(id);

        if (sector is null)
            return NotFound();

        return sector;
    }

    [HttpPost]
    public async Task<ActionResult<Sector>> Create(SectorCreateDto dto)
    {
        var (sector, conflict) = await sectorService.CreateAsync(dto);

        if (conflict)
            return Conflict("Sector already exists");

        return CreatedAtAction(nameof(Get), new { id = sector!.Id }, sector);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await sectorService.DeleteAsync(id))
            return NotFound();

        return NoContent();
    }
}
