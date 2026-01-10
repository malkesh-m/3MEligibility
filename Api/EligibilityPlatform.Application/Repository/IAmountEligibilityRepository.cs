using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing amount eligibility entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IAmountEligibilityRepository : IRepository<AmountEligibility>
    {
    }
}
