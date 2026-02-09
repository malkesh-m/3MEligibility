using AutoMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing NodeApi entities.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NodeApiService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class NodeApiService(IUnitOfWork uow, IMapper mapper) : INodeApiService
    {
        /// <summary>
        /// The unit of work instance for database operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The AutoMapper instance for object mapping.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new NodeApi record to the database.
        /// </summary>
        /// <param name="model">The NodeApiCreateOrUpdateModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(NodeApiCreateOrUpdateModel model)
        {
            // Sets NodeId to null if it is 0
            model.NodeId = model.NodeId == 0 ? (int?)null : model.NodeId;

            // Maps the incoming model to NodeApi entity
            var Entities = _mapper.Map<NodeApi>(model);
            Entities.CreatedByDateTime = DateTime.UtcNow;
            // Sets the update timestamp to current UTC time
            Entities.UpdatedByDateTime = DateTime.UtcNow;

            // Adds the entity to the repository
            _uow.NodeApiRepository.Add(Entities);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Adds a range of NodeApi records to the database.
        /// </summary>
        /// <param name="models">The list of NodeApiCreateOrUpdateModel to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRange(List<NodeApiCreateOrUpdateModel> models)
        {
            // Processes each model in the list
            foreach (var model in models)
            {
                // Sets NodeId to null if it is 0 for each model
                model.NodeId = model.NodeId == 0 ? (int?)null : model.NodeId;
            }

            // Maps the list of models to NodeApi entities
            var entities = _mapper.Map<List<NodeApi>>(models);

            // Sets update timestamp for each entity
            foreach (var entity in entities)
            {
                // Sets the update timestamp to current UTC time for each entity
                entity.UpdatedByDateTime = DateTime.UtcNow;
            }

            // Adds the range of entities to the repository
            _uow.NodeApiRepository.AddRange(entities);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a NodeApi record by its ID.
        /// </summary>
        /// <param name="id">The ID of the NodeApi record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieves the NodeApi entity by ID
            var nodes = _uow.NodeApiRepository.GetById(id);
            // Removes the entity from the repository
            _uow.NodeApiRepository.Remove(nodes);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all NodeApi records.
        /// </summary>
        /// <returns>A list of NodeApiListModel representing all NodeApi records.</returns>
        public List<NodeApiListModel> GetAll(int tenantId)
        {
            // Retrieves all NodeApi entities and *executes* the query immediately.
            var nodesApiList = _uow.NodeApiRepository
                                   .GetAllByTenantId(tenantId)
                                   .ToList();

            return _mapper.Map<List<NodeApiListModel>>(nodesApiList);
        }

        /// <summary>
        /// Gets a NodeApi record by its ID.
        /// </summary>
        /// <param name="id">The ID of the NodeApi record to retrieve.</param>
        /// <returns>The NodeApiListModel for the specified ID.</returns>
        public NodeApiListModel GetById(int id ,int tenantId)
        {
            // Retrieves the specific NodeApi entity by ID
            var nodes = _uow.NodeApiRepository.Query().Where(n=>n.TenantId==tenantId&&n.Apiid==id);
            // Maps the entity to NodeApiListModel object
            return _mapper.Map<NodeApiListModel>(nodes);
        }

        /// <summary>
        /// Gets all NodeApi records for a specific node ID.
        /// </summary>
        /// <param name="nodeId">The node ID.</param>
        /// <returns>A list of NodeApiListModel for the specified node ID.</returns>
        public List<NodeApiListModel> GetByNodeId(int nodeId)
        {
            // Queries NodeApi entities filtered by node ID
            var nodeApis = _uow.NodeApiRepository.Query()
                              .Where(x => x.NodeId == nodeId)
                              .ToList();

            // Maps the filtered entities to NodeApiListModel objects
            return _mapper.Map<List<NodeApiListModel>>(nodeApis);
        }

        /// <summary>
        /// Gets a single NodeApi record for a specific node ID.
        /// </summary>
        /// <param name="nodeId">The node ID.</param>
        /// <returns>The NodeApiListModel for the specified node ID.</returns>
        public NodeApiListModel GetByNodeIdSingle(int nodeId)
        {
            // Queries for the first NodeApi entity matching the node ID
            var nodeApi = _uow.NodeApiRepository.Query()
                            .FirstOrDefault(x => x.NodeId == nodeId);

            // Maps the entity to NodeApiListModel object
            return _mapper.Map<NodeApiListModel>(nodeApi);
        }

        /// <summary>
        /// Gets all NodeApi records by node ID.
        /// </summary>
        /// <param name="nodeId">The node ID.</param>
        /// <returns>A list of NodeApiListModel for the specified node ID.</returns>
        public List<NodeApiListModel> GetApiByNodeId(int nodeId)
        {
            // Queries NodeApi entities filtered by node ID
            var nodeEntities = _uow.NodeApiRepository.Query().Where(ni => ni.NodeId == nodeId).ToList();
            // Maps the filtered entities to NodeApiListModel objects
            return _mapper.Map<List<NodeApiListModel>>(nodeEntities);
        }

        /// <summary>
        /// Updates an existing NodeApi record.
        /// </summary>
        /// <param name="model">The NodeApiCreateOrUpdateModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(NodeApiCreateOrUpdateModel model)
        {
            // Sets NodeId to null if it is 0
            model.NodeId = model.NodeId == 0 ? (int?)null : model.NodeId;
            // Retrieves the existing NodeApi entity by ID
            var nodes = _uow.NodeApiRepository.GetById(model.Apiid);
            var createdBy = nodes.CreatedBy;
            // Maps the updated model to the existing entity
            var Entites = _mapper.Map<NodeApiCreateOrUpdateModel, NodeApi>(model, nodes);

            // Sets the update timestamp to current UTC time
            Entites.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the entity in the repository
            _uow.NodeApiRepository.Update(_mapper.Map<NodeApiCreateOrUpdateModel, NodeApi>(model, nodes));
            nodes.CreatedBy = createdBy;
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
        public async Task UpdateStatus(int Apiid, bool Isactive)
        {
            // Retrieve the existing NodeApi entity by ID
            var node = _uow.NodeApiRepository.GetById(Apiid) ?? throw new Exception("Node API not found");

            // Update only the Status from the model
            node.IsActive = Isactive; // model.Status should be "Active" or "Inactive"
            node.UpdatedByDateTime = DateTime.UtcNow;

            // Update repository (only the modified entity)
            _uow.NodeApiRepository.Update(node);

            // Commit changes
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple NodeApi records by their IDs.
        /// </summary>
        /// <param name="ids">A list of NodeApi IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when one or more provided IDs are not found in the database.</exception>
        public async Task MultipleDelete(List<int> ids)
        {
            // Validates that all provided IDs exist in the database
            foreach (var id in ids)
            {
                // Checks if the ID exists in the repository
                var hasvalue = await _uow.NodeApiRepository.Query().AnyAsync(item => item.Apiid == id);
                if (hasvalue == false)
                {
                    // Throws exception if ID is not found
                    throw new Exception($"These id's: {id} is not present. Please provide valid id.");
                }
            }

            // Deletes all entities with the provided IDs
            foreach (var id in ids)
            {
                // Retrieves the entity by ID
                var manageitem = _uow.NodeApiRepository.GetById(id);
                if (manageitem != null)
                {
                    // Removes the entity from the repository
                    _uow.NodeApiRepository.Remove(manageitem);
                }
            }

            // Commits the changes to the database with exception handling
            try
            {
                await _uow.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                // Throws exception with details if database update fails
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gets the binary XML for a NodeApi by its ID and node ID.
        /// </summary>
        /// <param name="id">The NodeApi ID.</param>
        /// <param name="nodeid">The node ID.</param>
        /// <returns>The binary XML string for the specified NodeApi and node ID.</returns>
        public string? GetBinaryXmlById(int id, int nodeid)
        {
            // Queries for the BinaryXml property of a specific NodeApi entity
            var result = _uow.NodeApiRepository.Query()
                .Where(n => n.Apiid == id && n.NodeId == nodeid)
                .Select(n => n.BinaryXml)
                .FirstOrDefault();

            // Returns the BinaryXml value or null if not found
            return result;
        }
    }
}
