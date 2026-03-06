namespace companyOSINT.Domain.Entities;

public class UserProject : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";

    public List<UserProjectCompany> Companies { get; set; } = [];
}
