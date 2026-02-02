using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MEligibilityPlatform.Domain.Entities
{
    public class ParameterBinding
    {
        [Key]
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int SystemParameterId { get; set; }

        public int? MappedParameterId { get; set; }

        [ForeignKey("MappedParameterId")]
        public virtual Parameter? MappedParameter { get; set; }
        public virtual SystemParameter? SystemParameter { get; set; }

    }
}
