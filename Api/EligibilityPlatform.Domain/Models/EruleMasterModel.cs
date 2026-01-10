using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EligibilityPlatform.Domain.Models
{
    public class EruleMasterModel
    {
        //public int Id { get; set; }

        public int? EruleId { get; set; }
        [Required(ErrorMessage = "Erule name is required.")]
        [StringLength(50, ErrorMessage = "Erule name cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Erule name contains invalid characters.")]
        public string? EruleName { get; set; }
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
    ErrorMessage = "Description allows only letters, numbers, and underscore")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
        [JsonIgnore]
        public int? EntityId { get; set; }

    }
    public class EruleMasterListModel : EruleMasterModel
    {
        public string? CreatedBy { get; set; }

        public DateTime? CreatedByDateTime { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedByDateTime { get; set; }
        public bool? IsActive { get; set; }

    }
    public class EruleMasterCreateUpodateModel : EruleMasterModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]

        public DateTime? CreatedByDateTime { get; set; }
        [JsonIgnore]

        public string? UpdatedBy { get; set; }
        [JsonIgnore]

        public DateTime? UpdatedByDateTime { get; set; }
        public bool? IsActive { get; set; }

    }

}
