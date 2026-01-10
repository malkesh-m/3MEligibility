using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EligibilityPlatform.Domain.Models
{
    public class DynamicApiRequest
    {
        [Required(ErrorMessage = "URL is required.")]
        [StringLength(2000, ErrorMessage = "URL cannot exceed 2000 characters.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string Url { get; set; } = string.Empty;

        [Required(ErrorMessage = "HTTP method is required.")]
        [RegularExpression("GET|POST|PUT|DELETE|PATCH", ErrorMessage = "Invalid HTTP method.")]
        public string HttpMethod { get; set; } = "GET";
        public Dictionary<string, object> Payload { get; set; } = [];
        public Dictionary<string, string>? Headers { get; set; }
    }
}
