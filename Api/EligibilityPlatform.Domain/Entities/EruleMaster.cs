using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EligibilityPlatform.Domain.Entities
{
    public class EruleMaster
    {
        public int Id { get; set; }

        public required string EruleName { get; set; }

        public string? EruleDesc { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedByDateTime { get; set; }

        public string? UpdatedBy { get; set; }

        public int EntityId { get; set; }

        public DateTime? UpdatedByDateTime { get; set; }

        public bool IsActive { get; set; }
        [ForeignKey("EntityId")]
        public virtual Entity? Entity { get; set; }

        public virtual ICollection<Erule> Erules { get; set; } = [];
    }
}
