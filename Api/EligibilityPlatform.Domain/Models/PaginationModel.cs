using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EligibilityPlatform.Domain.Models
{
    public class PaginationModel<T>
    {

        public bool IsSuccess { get; set; }
        public List<T> Data { get; set; } = [];
        public int TotalCount { get; set; }

    }
}
