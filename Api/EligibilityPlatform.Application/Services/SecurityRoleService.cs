using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for managing security roles.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SecurityRoleService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The Mapster mapper instance.</param>
    public class SecurityRoleService(IUnitOfWork uow, IMapper mapper) : ISecurityRoleService
    {
        private const string SuperAdminRoleName = "Super Admin";

        /// <summary>
        /// The unit of work instance for database operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The Mapster mapper instance for object mapping.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new security role to the database.
        /// </summary>
        /// <param name="securityRoleModel">The SecurityRoleModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(SecurityRoleUpdateModel securityRoleModel)
        {
            var normalizedName = (securityRoleModel.RoleName ?? "").Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw new InvalidOperationException("Role name is required.");
            }

            var exists = await _uow.SecurityRoleRepository.Query()
                .AnyAsync(sg => sg.TenantId == securityRoleModel.TenantId
                                && sg.RoleName != null
                                && sg.RoleName.ToLower() == normalizedName);
            if (exists)
            {
                throw new InvalidOperationException("This role already exists.");
            }

            securityRoleModel.CreatedByDateTime = DateTime.Now;
            securityRoleModel.UpdatedByDateTime = DateTime.UtcNow;
            _uow.SecurityRoleRepository.Add(_mapper.Map<SecurityRole>(securityRoleModel));
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all security roles.
        /// </summary>
        /// <returns>A list of SecurityRoleModel representing all security roles.</returns>
        public List<SecurityRoleModel> GetAll(int tenantId)
        {
            try
            {
                var securityRoles = _uow.SecurityRoleRepository.GetAllByTenantId(tenantId);
                
                if (securityRoles == null || !securityRoles.Any())
                {
                    return [];
                }
                
                return _mapper.Map<List<SecurityRoleModel>>(securityRoles.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting security roles for tenant {tenantId}: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Gets a security role by its ID.
        /// </summary>
        /// <param name="id">The security role ID to retrieve.</param>
        /// <returns>The SecurityRoleModel for the specified ID.</returns>
        public SecurityRoleModel GetById(int id, int tenantId)
        {
            var securityRole = _uow.SecurityRoleRepository.Query()
                .FirstOrDefault(s => s.RoleId == id && s.TenantId == tenantId) ?? throw new KeyNotFoundException($"Security Role with ID {id} not found for tenant {tenantId}");

            return _mapper.Map<SecurityRoleModel>(securityRole);
        }

        /// <summary>
        /// Removes a security role by its ID.
        /// </summary>
        /// <param name="id">The security role ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            try
            {
                var isSuperAdminRole = await _uow.SecurityRoleRepository.Query()
                    .AnyAsync(sg => sg.RoleId == id && sg.RoleName != null
                                    && sg.RoleName == SuperAdminRoleName);
                if (isSuperAdminRole)
                {
                    throw new InvalidOperationException("Super Admin role cannot be deleted.");
                }

                var item = _uow.SecurityRoleRepository.GetById(id);
                _uow.SecurityRoleRepository.Remove(item);
                await _uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing security role.
        /// </summary>
        /// <param name="securityRoleModel">The SecurityRoleModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(SecurityRoleUpdateModel securityRoleModel)
        {
            var isSuperAdminRole = await _uow.SecurityRoleRepository.Query()
                .AnyAsync(sg => sg.RoleId == securityRoleModel.RoleId && sg.RoleName != null
                                && sg.RoleName == SuperAdminRoleName);
            if (isSuperAdminRole)
            {
                throw new InvalidOperationException("Super Admin role cannot be edited.");
            }

            var item = _uow.SecurityRoleRepository.GetById(securityRoleModel.RoleId);
            securityRoleModel.UpdatedByDateTime = DateTime.UtcNow;
            var createdBy = item.CreatedBy;
            var createdByDateTime = item.CreatedByDateTime;
            _uow.SecurityRoleRepository.Update(_mapper.Map<SecurityRoleUpdateModel, SecurityRole>(securityRoleModel, item));

            item.CreatedBy = createdBy;
            item.CreatedByDateTime = createdByDateTime;
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple security roles by their IDs.
        /// </summary>
        /// <param name="ids">A list of security role IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            foreach (var id in ids)
            {
                var isSuperAdminRole = await _uow.SecurityRoleRepository.Query()
                    .AnyAsync(sg => sg.RoleId == id && sg.RoleName != null
                                    && sg.RoleName == SuperAdminRoleName);
                if (isSuperAdminRole)
                {
                    throw new InvalidOperationException("Super Admin role cannot be deleted.");
                }

                var hasvalue = await _uow.SecurityRoleRepository.Query().AnyAsync(item => item.RoleId == id);
                if (hasvalue == false)
                {
                    throw new Exception($"these  id's: {id} is not present. please provide valid id. ");
                }
            }

            foreach (var id in ids)
            {
                var manageitem = _uow.SecurityRoleRepository.GetById(id);
                if (manageitem != null)
                {
                    _uow.SecurityRoleRepository.Remove(manageitem);
                }
            }

            await _uow.CompleteAsync();
        }
    }
}
