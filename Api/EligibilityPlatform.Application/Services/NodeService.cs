using System.Xml.Linq;
using AutoMapper;
using MEligibilityPlatform.Application.Extensions;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing Node operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NodeService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class NodeService(IUnitOfWork uow, IMapper mapper) : INodeService
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
        /// Adds a new node to the database.
        /// </summary>
        /// <param name="model">The NodeCreateUpdateModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the node code already exists for the entity.</exception>
        public async Task Add(NodeCreateUpdateModel model)
        {
            // Retrieves all existing nodes for the entity to check for duplicate codes
            List<NodeListModel> results = GetAll(model.TenantId);
            // Checks if the provided code already exists for any node in the entity
            foreach (var result in results)
            {
                if (result.Code == model.Code)
                {
                    // Throws exception if code already exists
                    throw new InvalidOperationException("Code already exists");
                }
            }

            // Maps the incoming model to Node entity
            var Entity = _mapper.Map<Node>(model);
            // Sets the creation timestamp to current local time
            Entity.CreatedByDateTime = DateTime.Now;
            Entity.UpdatedByDateTime = DateTime.Now;
            // Adds the entity to the repository
            _uow.NodeModelRepository.Add(Entity);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a node by its entity ID and node ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The node ID to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int tenantId, int id)
        {
            // Retrieves the specific node by entity ID and node ID
            var nodes = _uow.NodeModelRepository.Query().First(f => f.NodeId == id && f.TenantId == tenantId);
            // Removes the node from the repository
            _uow.NodeModelRepository.Remove(nodes);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all nodes for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of NodeListModel representing all nodes for the entity.</returns>
        public List<NodeListModel> GetAll(int tenantId)
        {
            // Queries nodes filtered by entity ID and *executes* the query immediately.
            var nodesList = _uow.NodeModelRepository        
                                .GetAllByTenantId(tenantId)
                                .ToList();

            return _mapper.Map<List<NodeListModel>>(nodesList);
        }
        /// <summary>
        /// Gets a node by its entity ID and node ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The node ID to retrieve.</param>
        /// <returns>The NodeListModel for the specified entity and node ID.</returns>
        public NodeListModel GetById(int tenantId, int id)
        {
            // Retrieves the specific node by entity ID and node ID
            var node = _uow.NodeModelRepository.Query().FirstOrDefault(f => f.NodeId == id && f.TenantId == tenantId);
            // Maps the node to NodeListModel object
            return _mapper.Map<NodeListModel>(node);
        }

        /// <summary>
        /// Updates an existing node.
        /// </summary>
        /// <param name="model">The NodeCreateUpdateModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(NodeCreateUpdateModel model)
        {
            // Retrieves the existing node by entity ID and node ID
            var Item = _uow.NodeModelRepository.Query().First(f => f.NodeId == model.NodeId && f.TenantId == model.TenantId);
            // Maps the updated model to the existing entity
            var entity = _mapper.Map<NodeCreateUpdateModel, Node>(model, Item);
            // Sets the update timestamp to current UTC time
            entity.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the entity in the repository
            _uow.NodeModelRepository.Update(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple nodes by their IDs for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="ids">A list of node IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when one or more provided node IDs are not found for the entity.</exception>
        public async Task MultipleDelete(int tenantId, List<int> ids)
        {
            // Validates that all provided node IDs exist for the entity
            foreach (var id in ids)
            {
                // Checks if the node ID exists for the given entity
                var hasvalue = await _uow.NodeModelRepository.Query().AnyAsync(item => item.TenantId == tenantId && item.NodeId == id);
                if (hasvalue == false)
                {
                    // Throws exception if node ID is not found for the entity
                    throw new Exception($"These id's: {id} is not present. Please provide valid id.");
                }
            }

            // Deletes all nodes with the provided IDs for the entity
            foreach (var id in ids)
            {
                // Retrieves the specific node by entity ID and node ID
                var manageitem = _uow.NodeModelRepository.Query().First(f => f.NodeId == id && f.TenantId == tenantId);
                if (manageitem != null)
                {
                    // Removes the node from the repository
                    _uow.NodeModelRepository.Remove(manageitem);
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
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
