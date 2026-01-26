namespace MEligibilityPlatform.Domain.Models
{
    public class ApiParameterModel
    {
        public string Name { get; set; } = "";
        public string ActionUrl { get; set; } = "";
        public string Method { get; set; } = "";
        public List<ParameterDetail> Parameters { get; set; } = [];
        public ApiRequestBody? RequestBody { get; set; }
        public Dictionary<string, string> Responses { get; set; } = [];

        public List<OutputParameterDetail> OutputParameters { get; set; } = [];
        public List<ApiHeaderModel> Headers { get; set; } = [];
    }

    public class ParameterDetail
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
    }

    public class OutputParameterDetail
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
    }


    public class ApiRequestBody
    {
        public string Description { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public List<ParameterDetail> Parameters { get; set; } = [];
    }

    public class ApiHeaderModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Required { get; set; }
    }

}
