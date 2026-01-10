using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EligibilityPlatform.Domain.Models
{
    public class ProductCapAmountModel
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string? Activity { get; set; }
        //public decimal? MaxCapPerStream { get; set; }
        public string? Age { get; set; }
        public string? Salary { get; set; }
        public decimal Amount { get; set; }

    }

}
