using System.Data;
using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing role operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PermissionService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class PermissionService(IUnitOfWork uow, IMapper mapper) : IPermissionService
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
        /// Adds a new role to the database.
        /// </summary>
        /// <param name="roleModel">The PermissionModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(PermissionCreateUpdateModel permissionModel)
        {
            // Gets the last role to determine the next PermissionId
            var lastPermission = await _uow.PermissionRepository.GetLastPermission();
            // Sets the new PermissionId (increments from last role or starts at 1)
            int newId = lastPermission == null ? 1 : lastPermission.PermissionId + 1;
            permissionModel.PermissionId = newId;
            permissionModel.CreatedByDateTime = DateTime.Now;
            // Sets the update timestamp to current UTC time
            permissionModel.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to Permission entity and adds to repository
            _uow.PermissionRepository.Add(_mapper.Map<Permission>(permissionModel));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all roles.
        /// </summary>
        /// <returns>A list of PermissionModel representing all roles.</returns>
        public List<PermissionModel> GetAll()
        {
            // Retrieves all roles from the repository
            var roles = _uow.PermissionRepository.GetAll();
            // Maps the roles to PermissionModel objects
            return _mapper.Map<List<PermissionModel>>(roles);
        }

        /// <summary>
        /// Gets a role by its ID.
        /// </summary>
        /// <param name="id">The role ID to retrieve.</param>
        /// <returns>The PermissionModel for the specified ID.</returns>
        public PermissionModel GetById(int id)
        {
            // Retrieves the specific role by ID
            var permission = _uow.PermissionRepository.GetById(id);
            // Maps the role to PermissionModel object
            return _mapper.Map<PermissionModel>(permission);
        }

        /// <summary>
        /// Removes a role by its ID.
        /// </summary>
        /// <param name="id">The role ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves the role by ID
            var item = _uow.PermissionRepository.GetById(id);
            // Removes the role from the repository
            _uow.PermissionRepository.Remove(item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="roleModel">The PermissionModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(PermissionCreateUpdateModel permissionModel)
        {
            // Retrieves the existing role by ID
            var item = _uow.PermissionRepository.GetById(permissionModel.PermissionId);
            // Sets the update timestamp to current UTC time
            permissionModel.UpdatedByDateTime = DateTime.UtcNow;
            permissionModel.CreatedByDateTime = item.CreatedByDateTime;
            var createdBy = item.CreatedBy;

            // Updates the role with mapped data from the model
            _uow.PermissionRepository.Update(_mapper.Map<PermissionCreateUpdateModel, Permission>(permissionModel, item));
            item.CreatedBy = createdBy;
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}

