using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace MEligibilityPlatform.Domain.Entities;

public partial class ApiParameterMap : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    public int ApiParameterId { get; set; }

    public int ParameterId { get; set; }

    public DateTime LastModificationDate { get; set; }
    public int TenantId { get; set; }
    public int ApiId { get; set; }
    public virtual ApiParameter ApiParameter { get; set; } = null!;
    [ForeignKey(nameof(ApiId))]
    public virtual NodeApi NodeApi { get; set; } = null!;
    public virtual Parameter Parameter { get; set; } = null!;
}
