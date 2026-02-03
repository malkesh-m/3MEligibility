using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Entities
{
    public partial class ProductCapAmount : ITenantEntity
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        //public string? Activity {  get; set; }
        public decimal? MaxCapPerStream { get; set; }
        public string? Age { get; set; }
        public string? Salary { get; set; }
        public decimal Amount { get; set; }
        public int TenantId { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
