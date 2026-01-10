using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for evaluation history management operations.
    /// Provides methods for performing CRUD operations on evaluation history records.
    /// </summary>
    public interface IEvaluationHistoryService
    {
        /// <summary>
        /// Retrieves all evaluation history records for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity for which to retrieve evaluation history records.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="EvaluationHistoryModel"/> objects.</returns>
        Task<List<EvaluationHistoryModel>> GetAll(int entityId);

        /// <summary>
        /// Retrieves a specific evaluation history record by its identifier and entity identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the evaluation history record to retrieve.</param>
        /// <param name="entityId">The unique identifier of the entity associated with the evaluation history record.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="EvaluationHistoryModel"/> with the specified ID and entity ID.</returns>
        Task<EvaluationHistoryModel> GetById(int id, int entityId);

        /// <summary>
        /// Adds a new evaluation history record for a specific entity.
        /// </summary>
        /// <param name="model">The <see cref="EvaluationHistoryModel"/> containing the evaluation history details to add.</param>
        /// <param name="entityId">The unique identifier of the entity for which to add the evaluation history record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(EvaluationHistoryModel model, int entityId);

        /// <summary>
        /// Updates an existing evaluation history record.
        /// </summary>
        /// <param name="model">The <see cref="EvaluationHistoryModel"/> containing the updated evaluation history details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(EvaluationHistoryModel model);

        /// <summary>
        /// Deletes an evaluation history record by its identifier and entity identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the evaluation history record to delete.</param>
        /// <param name="entityId">The unique identifier of the entity associated with the evaluation history record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id, int entityId);
    }
}
