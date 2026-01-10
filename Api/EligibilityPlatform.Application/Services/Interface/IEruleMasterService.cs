using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for Erule master management operations.
    /// Provides methods for performing CRUD operations on Erule master records.
    /// </summary>
    public interface IEruleMasterService
    {
        /// <summary>
        /// Retrieves all Erule master records for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity for which to retrieve Erule master records.</param>
        /// <returns>A list of <see cref="EruleMasterListModel"/> objects containing all Erule master records for the specified entity.</returns>
        Task<List<EruleMasterListModel>> GetAll(int entityId);

        /// <summary>
        /// Retrieves a specific Erule master record by its identifier and entity identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the Erule master record to retrieve.</param>
        /// <param name="entityId">The unique identifier of the entity associated with the Erule master record.</param>
        /// <returns>The <see cref="EruleMasterModel"/> with the specified ID and entity ID.</returns>
        Task<EruleMasterModel> GetById(int id, int entityId);

        /// <summary>
        /// Adds a new Erule master record for a specific entity.
        /// </summary>
        /// <param name="model">The <see cref="EruleMasterCreateUpodateModel"/> containing the Erule master details to add.</param>
        /// <param name="entityId">The unique identifier of the entity for which to add the Erule master record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(EruleMasterCreateUpodateModel model, int entityId);

        /// <summary>
        /// Updates an existing Erule master record for a specific entity.
        /// </summary>
        /// <param name="model">The <see cref="EruleMasterCreateUpodateModel"/> containing the updated Erule master details.</param>
        /// <param name="entityId">The unique identifier of the entity associated with the Erule master record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Edit(EruleMasterCreateUpodateModel model, int entityId);
        Task<string> Delete(int id);
        Task<string> RemoveMultiple(int entityId, List<int> ids);


    }
}
