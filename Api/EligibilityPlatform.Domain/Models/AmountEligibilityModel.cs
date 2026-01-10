using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class AmountEligibilityModel
    {
        public int Id { get; set; }
        [Range(0, 100, ErrorMessage = "EligiblePercentage must be between 0 and 100")]

        public int EligiblePercentage { get; set; } //specify with "%" symbol
        [Range(0, 100, ErrorMessage = "AmountPercentage must be between 0 and 100")]

        public int AmountPrcentage { get; set; } //specify with "%" symbol
        [Range(1, int.MaxValue, ErrorMessage = "PcardID must be a positive number")]

        public int PcardID { get; set; }
    }
}
