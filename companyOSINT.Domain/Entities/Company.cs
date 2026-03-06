namespace companyOSINT.Domain.Entities;

public class Company : BaseEntity
{
    public string? Name { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? PostalCode { get; set; }
    public string? FederalState { get; set; }
    public string? RegisteredOffice { get; set; }
    public string? Registrar { get; set; }
    public string? RegisterArt { get; set; }
    public string? RegisterNummer { get; set; }
    public Guid? SectorId { get; set; }
    public Sector? Sector { get; set; }
    public string? Activity { get; set; }

    public DateTime? DateLastChecked { get; set; }

    public List<Contact> Contacts { get; set; } = [];
    public List<Website> Websites { get; set; } = [];
}
