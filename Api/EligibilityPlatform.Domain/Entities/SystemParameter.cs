using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Entities;


public class SystemParameter
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public virtual ICollection<ParameterBinding>? ParameterBindings { get; set; }
}
