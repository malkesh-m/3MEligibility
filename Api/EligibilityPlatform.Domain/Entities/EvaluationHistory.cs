using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace EligibilityPlatform.Domain.Entities
{
    public partial class EvaluationHistory
    {
        [Key]
        public int EvaluationHistoryId { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public string LoanNo { get; set; } = string.Empty;

        public DateTime EvaluationTimeStamp { get; set; }
        public string? Outcome { get; set; }
        public String? FailurReason { get; set; }
        public int CreditScore { get; set; }
        public int? PreviousApplication { get; set; }
        public double ProcessingTime { get; set; }
        //[ForeignKey("EntityId")]
        public int? TenantId { get; set; }
        public string? BreRequest { get; set; }
        public string? BreResponse { get; set; }


        //public virtual Entity? Entity { get; set; }
        //public virtual User? User { get; set; }
        public virtual ICollection<IntegrationApiEvaluation> IntegrationApiEvaluations { get; set; } = [];


    }
}
