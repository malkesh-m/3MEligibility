using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Models
{
    public class TenantModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Website { get; set; }

        public string ContactName { get; set; } = string.Empty;

        public int? BusinessTypeId { get; set; }

        public int CountryId { get; set; }

        public int CurrencyId { get; set; }

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int IndustryTypeId { get; set; }

        public string? Fax { get; set; }

        public string? LogoFilePath { get; set; }

        public string? LogoFileName { get; set; }

        public string? PhoneNo { get; set; }
    }

}
