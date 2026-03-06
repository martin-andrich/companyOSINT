namespace companyOSINT.Domain.Dtos.Companies;

public class CompanyPatchDto
{
    public Guid? SectorId { get; set; }
    public string? Activity { get; set; }
    public DateTime? DateLastChecked { get; set; }
}
