namespace companyOSINT.Domain.Entities;

public class Contact : BaseEntity
{
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MaidenName { get; set; }
    
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Position { get; set; }
    public string? Type { get; set; }
    public string? City { get; set; }
    public string? Flag { get; set; }
    
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Dismissed { get; set; }
    public string? ReferenceNo { get; set; }
}
