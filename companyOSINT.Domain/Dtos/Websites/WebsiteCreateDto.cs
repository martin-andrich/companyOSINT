namespace companyOSINT.Domain.Dtos.Websites;

public class WebsiteCreateDto
{
    public Guid CompanyId { get; set; }
    public string? UrlWebsite { get; set; }
    public string? UrlImprint { get; set; }
}
