using companyOSINT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Website> Websites { get; }
    DbSet<Software> Software { get; }
    DbSet<Tool> Tools { get; }
    DbSet<DomainToSkip> DomainsToSkip { get; }
    DbSet<Sector> Sectors { get; }
    DbSet<PostalCode> PostalCodes { get; }
    DbSet<UserProject> UserProjects { get; }
    DbSet<UserProjectCompany> UserProjectCompanies { get; }
    DbSet<ApiToken> ApiTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
