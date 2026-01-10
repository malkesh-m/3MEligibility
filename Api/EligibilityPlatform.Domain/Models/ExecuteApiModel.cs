using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class ExecuteApiModel
    {
        [Required]
        public int ApiId { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
