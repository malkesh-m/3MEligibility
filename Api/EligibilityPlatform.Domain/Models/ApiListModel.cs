using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EligibilityPlatform.Domain.Models
{

    public class ApiListModel
    {
        public int Apiid { get; set; }
        public string? Apiname { get; set; }
        public string EndpointPath { get; set; } = "";
        public string RequestBody { get; set; } = "";       // Template for POST/PUT
        public string RequestParameters { get; set; } = ""; // Template for GET/DELETE query
        public string ResponseRootPath { get; set; } = "";  // For mapping
        public string TargetTable { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public string? Apidesc { get; set; }
        public int? NodeId { get; set; }
        public string? NodeUrl { get; set; }                 // <-- Node URL included
        public string? HttpMethodType { get; set; }
        public string? BinaryXml { get; set; }
        public string? XmlfileName { get; set; }
        public string? Header { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedByDateTime { get; set; }


        public string ResponseFormate { get; set; } = string.Empty;
    }
}

