using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for maker-checker management operations.
    /// Provides methods for performing CRUD operations on maker-checker records.
    /// </summary>
    public interface IMakerCheckerService
    {
        /// <summary>
        /// Retrieves all maker-checker records.
        /// </summary>
        /// <returns>A list of <see cref="MakerCheckerModel"/> objects containing all maker-checker records.</returns>
        List<MakerCheckerModel> GetAll();

        /// <summary>
        /// Retrieves a specific maker-checker record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the maker-checker record to retrieve.</param>
        /// <returns>The <see cref="MakerCheckerModel"/> with the specified ID.</returns>
        MakerCheckerModel? GetById(int id);

        /// <summary>
        /// Adds a new maker-checker record.
        /// </summary>
        /// <param name="model">The <see cref="MakerCheckerModel"/> containing the maker-checker details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(MakerCheckerAddUpdateModel model);

        /// <summary>
        /// Updates an existing maker-checker record for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the maker-checker record.</param>
        /// <param name="model">The <see cref="MakerCheckerModel"/> containing the updated maker-checker details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(int tenantId, MakerCheckerModel model);

        /// <summary>
        /// Removes a maker-checker record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the maker-checker record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Adds a new maker-checker record using a copy model.
        /// </summary>
        /// <param name="modelCopy">The <see cref="MakerCheckerModelCopy"/> containing the maker-checker details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(MakerCheckerModelCopy modelCopy);
    }
}
