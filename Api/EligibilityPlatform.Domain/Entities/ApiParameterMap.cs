using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace EligibilityPlatform.Domain.Entities;

public partial class ApiParameterMap
{
    [Key]
    public int Id { get; set; }

    public int ApiParameterId { get; set; }

    public int ParameterId { get; set; }

    public DateTime LastModificationDate { get; set; }

    public virtual ApiParameter ApiParameter { get; set; } = null!;

    public virtual Parameter Parameter { get; set; } = null!;
}
