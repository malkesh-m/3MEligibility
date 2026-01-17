using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for managing amount eligibility operations.
    /// Provides methods for CRUD operations and amount calculation logic.
    /// </summary>
    public interface IAmountEligibilityService
    {
        /// <summary>
        /// Retrieves all amount eligibility records.
        /// </summary>
        /// <returns>A list of <see cref="AmountEligibilityModel"/> objects.</returns>
        List<AmountEligibilityModel> GetAll();

        /// <summary>
        /// Retrieves a specific amount eligibility record by its ID.
        /// </summary>
        /// <param name="id">The ID of the amount eligibility record to retrieve.</param>
        /// <returns>The <see cref="AmountEligibilityModel"/> with the specified ID.</returns>
        AmountEligibilityModel GetById(int id);

        /// <summary>
        /// Adds a new amount eligibility record.
        /// </summary>
        /// <param name="model">The <see cref="AmountEligibilityModel"/> to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(AmountEligibilityModel model);

        /// <summary>
        /// Updates an existing amount eligibility record.
        /// </summary>
        /// <param name="model">The <see cref="AmountEligibilityModel"/> with updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(AmountEligibilityModel model);

        /// <summary>
        /// Deletes an amount eligibility record by its ID.
        /// </summary>
        /// <param name="id">The ID of the amount eligibility record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Calculates the eligible amount based on entity ID, pre-amount, and PCard ID.
        /// </summary>
        /// <param name="tenantId">The ID of the entity.</param>
        /// <param name="Preamount">The pre-amount value to calculate from.</param>
        /// <param name="pcardId">The ID of the PCard.</param>
        /// <returns>A string representing the calculated eligible amount.</returns>
        string AmountCalculate(int tenantId, string Preamount, int pcardId);
    }
}
