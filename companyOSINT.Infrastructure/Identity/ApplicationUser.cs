using Microsoft.AspNetCore.Identity;

namespace companyOSINT.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public DateTime DateCreated { get; set; }
}
