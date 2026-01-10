namespace EligibilityPlatform.Domain.Models
{
    public class BestFitProductModel
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int PcardId { get; set; }
        public required string PcardName { get; set; }
    }
}
