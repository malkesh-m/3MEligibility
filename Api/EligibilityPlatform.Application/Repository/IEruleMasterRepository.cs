using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing E-Rule master entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IEruleMasterRepository : IRepository<EruleMaster>
    {
    }
}
