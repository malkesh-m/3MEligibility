using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing maker-checker entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IMakerCheckerRepository : IRepository<MakerChecker>
    {
    }
}
