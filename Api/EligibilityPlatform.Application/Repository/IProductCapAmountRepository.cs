using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing product cap amount entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IProductCapAmountRepository : IRepository<ProductCapAmount>
    {
    }
}
