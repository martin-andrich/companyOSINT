namespace companyOSINT.Domain.Entities;

public class Software : BaseEntity
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string FoundAt { get; set; } = "";
    public Guid WebsiteId { get; set; }
    public Website Website { get; set; } = null!;
    public DateTime? DateLastChecked { get; set; }
}
