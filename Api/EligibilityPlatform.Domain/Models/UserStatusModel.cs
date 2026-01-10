using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class UserStatusModel
    {
        public int StatusId { get; set; }

        [Required(ErrorMessage = "Status Name is required.")]
        [StringLength(50, ErrorMessage = "Status Name cannot be longer than 50 characters.")]
        public string? StatusName { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
    }
    public class UserStatusAddModel
    {
        public int StatusId { get; set; }

        [Required(ErrorMessage = "Status Name is required.")]
        [StringLength(50, ErrorMessage = "Status Name cannot be longer than 50 characters.")]
        public string? StatusName { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
    }
}
