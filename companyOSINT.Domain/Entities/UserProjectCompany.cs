using companyOSINT.Domain.Enums;

namespace companyOSINT.Domain.Entities;

public class UserProjectCompany : BaseEntity
{
    public Guid ProjectId { get; set; }
    public UserProject Project { get; set; } = null!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ProjectCompanyStatus Status { get; set; } = ProjectCompanyStatus.Neu;
}
