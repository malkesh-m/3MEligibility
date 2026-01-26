using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing mapping function entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IMappingFunctionRepository : IRepository<MappingFunction>
    {
    }
}
