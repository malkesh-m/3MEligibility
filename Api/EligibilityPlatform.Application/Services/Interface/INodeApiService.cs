using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for node API management operations.
    /// Provides methods for performing CRUD operations and bulk actions on node API records.
    /// </summary>
    public interface INodeApiService
    {
        /// <summary>
        /// Retrieves all node API records.
        /// </summary>
        /// <returns>A list of <see cref="NodeApiListModel"/> objects containing all node API records.</returns>
        List<NodeApiListModel> GetAll();

        /// <summary>
        /// Retrieves a specific node API record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the node API record to retrieve.</param>
        /// <returns>The <see cref="NodeApiListModel"/> with the specified ID.</returns>
        NodeApiListModel GetById(int id);

        /// <summary>
        /// Adds a new node API record.
        /// </summary>
        /// <param name="model">The <see cref="NodeApiCreateOrUpdateModel"/> containing the node API details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(NodeApiCreateOrUpdateModel model);

        /// <summary>
        /// Updates an existing node API record.
        /// </summary>
        /// <param name="model">The <see cref="NodeApiCreateOrUpdateModel"/> containing the updated node API details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(NodeApiCreateOrUpdateModel model);
        Task UpdateStatus(int Apiid, bool Isactive);

        /// <summary>
        /// Deletes a node API record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the node API record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple node API records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the node API records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);

        /// <summary>
        /// Retrieves binary XML data for a specific node API record.
        /// </summary>
        /// <param name="id">The unique identifier of the node API record.</param>
        /// <param name="nodeid">The unique identifier of the associated node.</param>
        /// <returns>A string containing the binary XML data.</returns>
        string? GetBinaryXmlById(int id, int nodeid);

        /// <summary>
        /// Adds multiple node API records in a single operation.
        /// </summary>
        /// <param name="models">A list of <see cref="NodeApiCreateOrUpdateModel"/> objects containing the node API details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRange(List<NodeApiCreateOrUpdateModel> models);

        /// <summary>
        /// Retrieves all node API records associated with a specific node identifier.
        /// </summary>
        /// <param name="Nodeid">The unique identifier of the node.</param>
        /// <returns>A list of <see cref="NodeApiListModel"/> objects associated with the specified node.</returns>
        List<NodeApiListModel> GetApiByNodeId(int Nodeid);

        /// <summary>
        /// Retrieves all node API records associated with a specific node identifier.
        /// </summary>
        /// <param name="nodeId">The unique identifier of the node.</param>
        /// <returns>A list of <see cref="NodeApiListModel"/> objects associated with the specified node.</returns>
        List<NodeApiListModel> GetByNodeId(int nodeId);

        /// <summary>
        /// Retrieves a single node API record associated with a specific node identifier.
        /// </summary>
        /// <param name="nodeId">The unique identifier of the node.</param>
        /// <returns>A single <see cref="NodeApiListModel"/> object associated with the specified node.</returns>
        NodeApiListModel GetByNodeIdSingle(int nodeId);
    }
}
