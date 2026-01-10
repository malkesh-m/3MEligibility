namespace EligibilityPlatform.Domain.Models
{
    public class ApiAnalysisResult
    {
        public required string Url { get; set; }
        public required string Method { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public Dictionary<string, string?> Headers { get; set; } = [];
        public object ResponseBody { get; set; } = string.Empty;
    }

    public class ApiAnalysisRequest
    {
        public required string ApiUrl { get; set; }
        public string Method { get; set; } = "GET";
        public object? RequestBody { get; set; }
    }

}
