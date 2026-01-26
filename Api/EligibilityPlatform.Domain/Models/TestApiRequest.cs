using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Models
{
    public class TestApiRequest
    {
        public int ApiId { get; set; }
        public string? FullUrl { get; set; }
        public string? HttpMethod { get; set; }
        public string? InputJson { get; set; }
    }
}
