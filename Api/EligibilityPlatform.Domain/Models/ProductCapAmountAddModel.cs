using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Models
{
    public class ProductCapAmountAddModel
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public int? ProductId { get; set; }
        [RegularExpression(
    @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
    ErrorMessage = "Parameter name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [StringLength(100, ErrorMessage = "Activity cannot exceed 100 characters.")]
        public string? Activity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MaxCapPerStream must be non-negative.")]
        public decimal? MaxCapPerStream { get; set; }

        public string? Age { get; set; }

        public string? Salary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public decimal Amount { get; set; }
        [JsonIgnore]
        public int TenantId { get; set; }
    }
    public class ProductCapAmountUpdateModel : ProductCapAmountAddModel
    {
        [Required(ErrorMessage = "Id is required for update.")]

        public int Id { get; set; }

    }

}
