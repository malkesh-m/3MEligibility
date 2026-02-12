using System.Data;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing role entities.
    /// Extends the base repository interface with additional role-specific operations.
    /// </summary>
    public interface IPermissionRepository : IRepository<Permission>
    {
        /// <summary>
        /// Retrieves the most recently created or modified role.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the last <see cref="Permission"/> entity.</returns>
        Task<Permission?> GetLastPermission();
    }
}

