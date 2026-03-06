namespace companyOSINT.Domain.Entities;

public class Website : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string? UrlWebsite { get; set; }
    public string? UrlImprint { get; set; }

    public int HttpResponseCode { get; set; }
    public bool IsSubdomain { get; set; }
    public string? IpAddress { get; set; }
    public bool SslValid { get; set; }
    public bool ConsentManagerFound { get; set; }
    public int RequestsWithoutConsent { get; set; }
    public int CookiesWithoutConsent { get; set; }
    public double AverageTimeToFirstByte { get; set; }

    public DateTime? DateLastChecked { get; set; }

    public List<Software> Softwares { get; set; } = [];
    public List<Tool> Tools { get; set; } = [];
}
