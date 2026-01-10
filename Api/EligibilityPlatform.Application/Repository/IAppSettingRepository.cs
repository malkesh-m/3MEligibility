using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing application setting entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IAppSettingRepository : IRepository<AppSetting>
    {
    }
}
