using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EligibilityPlatform.Domain.Models
{
    public class UserActivityModel
    {
        public string? ActionType { get; set; }
        public string? ActionName { get; set; }
        public string? ComponentName { get; set; }
        public string? PageUrl { get; set; }

    }
}
