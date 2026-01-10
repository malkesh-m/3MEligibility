namespace EligibilityPlatform.Domain.Models
{
    public class ExceptionParameterModel
    {
        public int Id { get; set; }
        public int ParameterId { get; set; }
        public string? ParameterValue1 { get; set; }
        public string? ParameterValue2 { get; set; }
        public int ExceptionId { get; set; }

    }
}
