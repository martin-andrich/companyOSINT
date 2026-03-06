using companyOSINT.Domain.Enums;

namespace companyOSINT.Domain.Dtos.Projects;

public class ProjectCompanyDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? Website { get; set; }
    public ProjectCompanyStatus Status { get; set; }
}
