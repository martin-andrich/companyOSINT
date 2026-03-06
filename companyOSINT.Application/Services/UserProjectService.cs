using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Dtos.Projects;
using companyOSINT.Domain.Entities;
using companyOSINT.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Services;

public class UserProjectService(IApplicationDbContext db) : IUserProjectService
{
    public async Task<List<UserProject>> GetProjectsByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.UserProjects
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<UserProject?> GetByIdAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        return await db.UserProjects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);
    }

    public async Task<UserProject> CreateAsync(Guid userId, UserProjectCreateDto dto, CancellationToken ct = default)
    {
        var project = new UserProject
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
        };

        db.UserProjects.Add(project);
        await db.SaveChangesAsync(ct);

        return project;
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        var project = await db.UserProjects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);

        if (project is null) return false;

        db.UserProjects.Remove(project);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ProjectCompanyDto>> GetProjectCompaniesAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        var ownsProject = await db.UserProjects
            .AnyAsync(p => p.Id == projectId && p.UserId == userId, ct);
        if (!ownsProject) return [];

        return await db.UserProjectCompanies
            .Where(pc => pc.ProjectId == projectId)
            .OrderBy(pc => pc.Company.Name)
            .Select(pc => new ProjectCompanyDto
            {
                Id = pc.Id,
                CompanyId = pc.CompanyId,
                CompanyName = pc.Company.Name,
                RegisteredAddress = pc.Company.RegisteredAddress,
                Website = pc.Company.Websites
                    .Where(w => w.UrlWebsite != null && w.UrlWebsite != "")
                    .Select(w => w.UrlWebsite)
                    .FirstOrDefault(),
                Status = pc.Status,
            })
            .ToListAsync(ct);
    }

    public async Task<(UserProjectCompany? Entry, bool AlreadyExists)> AddCompanyAsync(
        Guid userId, AddCompanyToProjectDto dto, CancellationToken ct = default)
    {
        var ownsProject = await db.UserProjects
            .AnyAsync(p => p.Id == dto.ProjectId && p.UserId == userId, ct);
        if (!ownsProject) return (null, false);

        var exists = await db.UserProjectCompanies
            .AnyAsync(pc => pc.ProjectId == dto.ProjectId && pc.CompanyId == dto.CompanyId, ct);
        if (exists) return (null, true);

        var entry = new UserProjectCompany
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            CompanyId = dto.CompanyId,
            Status = ProjectCompanyStatus.Neu,
        };

        db.UserProjectCompanies.Add(entry);
        await db.SaveChangesAsync(ct);

        return (entry, false);
    }

    public async Task<bool> UpdateCompanyStatusAsync(
        Guid entryId, Guid userId, UpdateCompanyStatusDto dto, CancellationToken ct = default)
    {
        var entry = await db.UserProjectCompanies
            .Include(pc => pc.Project)
            .FirstOrDefaultAsync(pc => pc.Id == entryId && pc.Project.UserId == userId, ct);

        if (entry is null) return false;

        entry.Status = dto.Status;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RemoveCompanyAsync(Guid entryId, Guid userId, CancellationToken ct = default)
    {
        var entry = await db.UserProjectCompanies
            .Include(pc => pc.Project)
            .FirstOrDefaultAsync(pc => pc.Id == entryId && pc.Project.UserId == userId, ct);

        if (entry is null) return false;

        db.UserProjectCompanies.Remove(entry);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
