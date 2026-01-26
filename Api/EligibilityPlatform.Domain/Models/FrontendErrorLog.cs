using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Models
{
    public class FrontendErrorLog
    {
        public string? User { get; set; }
        public required string Component { get; set; }
        public required string Path { get; set; }
        public required string Message { get; set; }
        public required string Stack { get; set; }
        public string? Request { get; set; }
        public int Status { get; set; }
        //public DateTime TimeStamp { get; set; }
        public required string UserAgent { get; set; }
    }
}
