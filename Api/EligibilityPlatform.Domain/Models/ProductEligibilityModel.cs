namespace EligibilityPlatform.Domain.Models
{
    public class ProductEligibilityModel
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        //public int CreditScore { get; set; }
        public decimal EligibleAmount { get; set; }
        //public double EligibilityPercentage { get; set; }
    }
}
