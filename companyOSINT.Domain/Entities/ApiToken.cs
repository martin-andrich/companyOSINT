namespace companyOSINT.Domain.Entities;

public class ApiToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";
    public string TokenHash { get; set; } = "";
    public string TokenPrefix { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
