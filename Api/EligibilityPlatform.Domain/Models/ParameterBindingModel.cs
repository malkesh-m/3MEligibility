using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Domain.Models
{
    public class ParameterBindingModel
    {

        public int Id { get; set; }

        public int TenantId { get; set; }

        public int SystemParameterId { get; set; }

        public int? MappedParameterId { get; set; }
        public string? SystemParameterName { get; set; }
    }
     public class ParameterBindingAddModel
        {

            public int Id { get; set; }

            public int TenantId { get; set; }

            public int SystemParameterId { get; set; }

            public int? MappedParameterId { get; set; }

        }
    }

