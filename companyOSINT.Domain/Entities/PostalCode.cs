namespace companyOSINT.Domain.Entities;

public class PostalCode
{
    public string Code { get; set; } = "";
    public string Place { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
