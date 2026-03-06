using companyOSINT.Domain.Dtos.Projects;
using companyOSINT.Domain.Entities;

namespace companyOSINT.Application.Interfaces;

public interface IUserProjectService
{
    Task<List<UserProject>> GetProjectsByUserAsync(Guid userId, CancellationToken ct = default);
    Task<UserProject?> GetByIdAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    Task<UserProject> CreateAsync(Guid userId, UserProjectCreateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    Task<List<ProjectCompanyDto>> GetProjectCompaniesAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    Task<(UserProjectCompany? Entry, bool AlreadyExists)> AddCompanyAsync(Guid userId, AddCompanyToProjectDto dto, CancellationToken ct = default);
    Task<bool> UpdateCompanyStatusAsync(Guid entryId, Guid userId, UpdateCompanyStatusDto dto, CancellationToken ct = default);
    Task<bool> RemoveCompanyAsync(Guid entryId, Guid userId, CancellationToken ct = default);
}
