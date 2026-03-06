namespace companyOSINT.Domain.Dtos.Websites;

public class WebsitePatchDto
{
    public int? HttpResponseCode { get; set; }
    public string? IpAddress { get; set; }
    public bool? SslValid { get; set; }
    public double? AverageTimeToFirstByte { get; set; }
    public DateTime? DateLastChecked { get; set; }
    public bool? ConsentManagerFound { get; set; }
    public int? RequestsWithoutConsent { get; set; }
    public int? CookiesWithoutConsent { get; set; }
}
