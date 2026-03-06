namespace companyOSINT.Domain.Dtos.Projects;

public class AddCompanyToProjectDto
{
    public Guid ProjectId { get; set; }
    public Guid CompanyId { get; set; }
}
