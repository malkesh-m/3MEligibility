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
    /// Initializes a new instance of the <see cref="RoleService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class RoleService(IUnitOfWork uow, IMapper mapper) : IRoleService
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
        /// <param name="roleModel">The RoleModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(RoleCreateUpdateModel roleModel)
        {
            // Gets the last role to determine the next RoleId
            var lastRole = await _uow.RoleRepository.GetLastRole();
            // Sets the new RoleId (increments from last role or starts at 1)
            int newId = lastRole == null ? 1 : lastRole.RoleId + 1;
            roleModel.RoleId = newId;
            roleModel.CreatedByDateTime = DateTime.Now;
            // Sets the update timestamp to current UTC time
            roleModel.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to Role entity and adds to repository
            _uow.RoleRepository.Add(_mapper.Map<Role>(roleModel));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all roles.
        /// </summary>
        /// <returns>A list of RoleModel representing all roles.</returns>
        public List<RoleModel> GetAll()
        {
            // Retrieves all roles from the repository
            var roles = _uow.RoleRepository.GetAll();
            // Maps the roles to RoleModel objects
            return _mapper.Map<List<RoleModel>>(roles);
        }

        /// <summary>
        /// Gets a role by its ID.
        /// </summary>
        /// <param name="id">The role ID to retrieve.</param>
        /// <returns>The RoleModel for the specified ID.</returns>
        public RoleModel GetById(int id)
        {
            // Retrieves the specific role by ID
            var role = _uow.RoleRepository.GetById(id);
            // Maps the role to RoleModel object
            return _mapper.Map<RoleModel>(role);
        }

        /// <summary>
        /// Removes a role by its ID.
        /// </summary>
        /// <param name="id">The role ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves the role by ID
            var item = _uow.RoleRepository.GetById(id);
            // Removes the role from the repository
            _uow.RoleRepository.Remove(item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="roleModel">The RoleModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(RoleCreateUpdateModel roleModel)
        {
            // Retrieves the existing role by ID
            var item = _uow.RoleRepository.GetById(roleModel.RoleId);
            // Sets the update timestamp to current UTC time
            roleModel.UpdatedByDateTime = DateTime.UtcNow;
            roleModel.CreatedByDateTime = item.CreatedByDateTime;
            var createdBy = item.CreatedBy;

            // Updates the role with mapped data from the model
            _uow.RoleRepository.Update(_mapper.Map<RoleCreateUpdateModel, Role>(roleModel, item));
            item.CreatedBy = createdBy;
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
