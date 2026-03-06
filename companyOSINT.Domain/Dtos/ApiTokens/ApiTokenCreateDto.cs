namespace companyOSINT.Domain.Dtos.ApiTokens;

public class ApiTokenCreateDto
{
    public string Name { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}
