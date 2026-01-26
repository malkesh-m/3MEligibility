using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for node management operations.
    /// Provides methods for performing CRUD operations and bulk actions on node records.
    /// </summary>
    public interface INodeService
    {
        /// <summary>
        /// Retrieves all node records for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="NodeListModel"/> objects containing all node records for the specified entity.</returns>
        List<NodeListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific node record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the node record to retrieve.</param>
        /// <returns>The <see cref="NodeListModel"/> with the specified ID within the given entity.</returns>
        NodeListModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new node record.
        /// </summary>
        /// <param name="model">The <see cref="NodeCreateUpdateModel"/> containing the node details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(NodeCreateUpdateModel model);

        /// <summary>
        /// Updates an existing node record.
        /// </summary>
        /// <param name="model">The <see cref="NodeCreateUpdateModel"/> containing the updated node details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(NodeCreateUpdateModel model);

        /// <summary>
        /// Deletes a node record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the node record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int tenantId, int id);

        /// <summary>
        /// Deletes multiple node records within a specific entity in a single operation.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="ids">A list of unique identifiers of the node records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(int tenantId, List<int> ids);
    }
}
