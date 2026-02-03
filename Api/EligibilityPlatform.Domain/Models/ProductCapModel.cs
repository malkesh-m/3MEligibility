using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class ProductCapModel
    {
        public int Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MinimumScore must be 0 or greater.")]
        public int MinimumScore { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaximumScore must be 0 or greater.")]
        public int MaximumScore { get; set; }

        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Range(0, 100, ErrorMessage = "ProductCapPercentage must be between 0 and 100.")]
        public decimal ProductCapPercentage { get; set; }
        public int TenantId { get; set; }


    }
}
