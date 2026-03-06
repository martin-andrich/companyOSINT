namespace companyOSINT.Domain.Entities;

public class Tool : BaseEntity
{
    public string Name { get; set; } = "";
    public string FoundAt { get; set; } = "";

    DateTime DateLastChecked { get; set; }
    public Guid WebsiteId { get; set; }
    public Website Website { get; set; } = null!;
}
