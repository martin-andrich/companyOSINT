namespace companyOSINT.Domain.Common;

public class CompanySearchResultDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? Website { get; set; }
    public string? Activity { get; set; }
}
