namespace companyOSINT.Domain.Common;

public class CompanyListDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? FederalState { get; set; }
    public string? RegisteredOffice { get; set; }
    public string? Registrar { get; set; }
    public string? RegisterArt { get; set; }
    public string? RegisterNummer { get; set; }
    public string? Activity { get; set; }
    public Guid? SectorId { get; set; }
    public string? SectorName { get; set; }
    public DateTime? DateLastChecked { get; set; }
}
