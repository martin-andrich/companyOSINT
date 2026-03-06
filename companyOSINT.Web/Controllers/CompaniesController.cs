using companyOSINT.Domain.Dtos.Companies;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Common;
using companyOSINT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace companyOSINT.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(ICompanyService companyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CompanyListDto>>> GetAll(
        [FromQuery] string? name,
        [FromQuery] string? federalState,
        [FromQuery] string? registrar,
        [FromQuery] string? search,
        [FromQuery] Guid? sectorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        return await companyService.GetAllAsync(name, federalState, registrar, search, sectorId, page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Company>> Get(Guid id)
    {
        var company = await companyService.GetByIdAsync(id);

        if (company is null)
            return NotFound();

        return company;
    }

    [HttpPost]
    public async Task<ActionResult<Company>> Create(Company company)
    {
        var created = await companyService.CreateAsync(company);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Company company)
    {
        if (id != company.Id)
            return BadRequest();

        if (!await companyService.UpdateAsync(id, company))
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await companyService.DeleteAsync(id))
            return NotFound();

        return NoContent();
    }

    [HttpGet("names-to-check")]
    public async Task<ActionResult<CursorPage<CompanyNameDto>>> GetNamesToCheck(
        [FromQuery] Guid? afterId,
        [FromQuery] int pageSize = 100_000)
    {
        return await companyService.GetNamesToCheckAsync(afterId, pageSize);
    }

    [HttpGet("next-to-check")]
    public async Task<IActionResult> GetNextToCheck()
    {
        var company = await companyService.GetNextToCheckAsync();

        if (company is null)
            return NoContent();

        return Ok(company);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, CompanyPatchDto dto)
    {
        if (!await companyService.PatchAsync(id, dto))
            return NotFound();

        return NoContent();
    }
}
