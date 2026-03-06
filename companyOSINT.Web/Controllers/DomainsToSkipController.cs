using companyOSINT.Domain.Dtos.Domains;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace companyOSINT.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainsToSkipController(IDomainToSkipService domainToSkipService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DomainToSkip>>> GetAll()
    {
        return await domainToSkipService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DomainToSkip>> Get(Guid id)
    {
        var domain = await domainToSkipService.GetByIdAsync(id);

        if (domain is null)
            return NotFound();

        return domain;
    }

    [HttpPost]
    public async Task<ActionResult<DomainToSkip>> Create(DomainToSkipCreateDto dto)
    {
        var (domain, conflict) = await domainToSkipService.CreateAsync(dto);

        if (conflict)
            return Conflict("Domain already exists");

        return CreatedAtAction(nameof(Get), new { id = domain!.Id }, domain);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await domainToSkipService.DeleteAsync(id))
            return NotFound();

        return NoContent();
    }
}
