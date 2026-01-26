using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Entities
{
    public class IntegrationApiEvaluation
    {
        public int Id { get; set; }

        public int? EvaluationHistoryId { get; set; }

        public int NodeApiId { get; set; }
        public string? ApiRequest { get; set; }
        public string? ApiResponse { get; set; }

        public DateTime EvaluationTimeStamp { get; set; } = DateTime.Now;
        public virtual NodeApi? NodeApi { get; set; }
        public virtual EvaluationHistory? EvaluationHistory { get; set; }


    }
}
