using System.Xml.Linq;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing node model entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface INodeModelRepository : IRepository<Node>
    {
    }
}
