using System.ComponentModel.DataAnnotations.Schema;

namespace companyOSINT.Domain.Entities;

public abstract class BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}
