using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing role-permission associations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RolePermissionService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The Mapster mapper instance.</param>
    public class RolePermissionService(IUnitOfWork uow, IMapper mapper, IMemoryCache cache, IUserService userService, IUserContextService userContext, IUserRoleService userRoleService) : IRolePermissionService
    {
        private const string SuperAdminRoleName = "Super Admin";

        private readonly IUnitOfWork _uow = uow;
        private readonly IUserService _userService = userService;
        private readonly IUserContextService _userContext = userContext;
        private readonly IMapper _mapper = mapper;
        private readonly IMemoryCache _cache = cache;
        private readonly IUserRoleService _userRoleService = userRoleService;

        /// <summary>
        /// Adds role-permission assignments based on the given model.
        /// </summary>
        /// <param name="rolePermissionModel">
        /// The model containing the role ID and list of permissions to assign.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(RolePermissionModel rolePermissionModel)
        {
            await EnsureCanEditRolePermissions(rolePermissionModel.RoleId, rolePermissionModel.TenantId);

            var existingPermissionIds = await _uow.RolePermissionRepository.Query()
                .Where(x => x.RoleId == rolePermissionModel.RoleId && x.TenantId == rolePermissionModel.TenantId)
                .Select(x => x.PermissionId)
                .ToListAsync();

            var rolePermissions = rolePermissionModel.PermissionIds
                .Where(permissionId => !existingPermissionIds.Contains(permissionId))
                .Select(permissionId => new RolePermission
                {
                    RoleId = rolePermissionModel.RoleId,
                    PermissionId = permissionId,
                    TenantId = rolePermissionModel.TenantId,
                    UpdatedByDateTime = DateTime.UtcNow
                })
                .ToList();

            if (rolePermissions.Count != 0)
            {
                _uow.RolePermissionRepository.AddRange(rolePermissions);
                await _uow.CompleteAsync();
            }

            var userIds = await _uow.UserRoleRepository.Query()
              .Where(x => x.RoleId == rolePermissionModel.RoleId)
             .Select(x => x.UserId)
             .ToListAsync();
            foreach (var userId in userIds)
            {
                _userService.RemoveUserPermissionsCache(userId);
            }
        }

        /// <summary>
        /// Retrieves all role-permission mappings.
        /// </summary>
        /// <returns>A list of <see cref="RolePermissionModel"/> instances.</returns>
        public List<RolePermissionModel> GetAll()
        {
            var rolePermissions = _uow.RolePermissionRepository.GetAll();
            return _mapper.Map<List<RolePermissionModel>>(rolePermissions);
        }

        /// <summary>
        /// Determines whether any permissions are associated with the given security role ID.
        /// </summary>
        /// <param name="roleId">The ID of the role to check.</param>
        /// <returns>
        /// True if at least one permission is associated; otherwise, false.
        /// </returns>
        public async Task<bool> GetBySecurityRoleId(int roleId)
        {
            return await _uow.RolePermissionRepository.GetBySecurityRoleId(roleId);
        }

        /// <summary>
        /// Removes specified permissions from a given role.
        /// </summary>
        /// <param name="rolePermissionModel">
        /// The model containing the role ID and list of permissions to remove.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(RolePermissionModel rolePermissionModel)
        {
            await EnsureCanEditRolePermissions(rolePermissionModel.RoleId, rolePermissionModel.TenantId);

            var isSuperAdminRole = await _uow.SecurityRoleRepository.Query()
                .AnyAsync(sg => sg.RoleId == rolePermissionModel.RoleId
                                && sg.TenantId == rolePermissionModel.TenantId
                                && sg.RoleName != null
                                && sg.RoleName == SuperAdminRoleName);
            if (isSuperAdminRole)
            {
                throw new InvalidOperationException("Super Admin role permissions cannot be removed.");
            }

            var itemsToRemove = new List<RolePermission>();

            foreach (var permissionId in rolePermissionModel.PermissionIds)
            {
                var item = await _uow.RolePermissionRepository
                    .GetRolePermission(rolePermissionModel.RoleId, permissionId);

                if (item != null)
                {
                    itemsToRemove.Add(item);
                }
            }

            if (itemsToRemove.Count != 0)
            {
                _uow.RolePermissionRepository.RemoveRange(itemsToRemove);
            }
            await _uow.CompleteAsync();

            var userIds = await _uow.UserRoleRepository.Query()
            .Where(x => x.RoleId == rolePermissionModel.RoleId)
            .Select(x => x.UserId)
            .ToListAsync();
            foreach (var userId in userIds)
            {
                _userService.RemoveUserPermissionsCache(userId);
            }
        }

        /// <summary>
        /// Retrieves permissions assigned to the specified role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>
        /// A list of <see cref="AssignedPermissionModel"/> representing assigned permissions.
        /// </returns>
        public async Task<IList<AssignedPermissionModel>> GetAssignedPermissions(int roleId,int tenantId)
        {
            return await _uow.RolePermissionRepository.Query()
                .Include(i => i.Permission)
                .Where(w => w.RoleId == roleId &&w.TenantId==tenantId)
                .Select(s => new AssignedPermissionModel
                {
                    RoleId = s.RoleId,
                    PermissionAction = s.Permission.PermissionAction ?? "",
                    PermissionId = s.PermissionId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves permissions not currently assigned to the specified role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>
        /// A list of <see cref="AssignedPermissionModel"/> representing unassigned permissions.
        /// </returns>
        public async Task<IList<AssignedPermissionModel>> GetUnAssignedPermissions(int roleId,int tenantId)
        {
            var assignedPermissionIds = _uow.RolePermissionRepository.Query()
                .Where(w => w.RoleId == roleId &&w.TenantId==tenantId)
                .Select(s => s.PermissionId);

            return await _uow.PermissionRepository.Query()
                .Where(w => !assignedPermissionIds.Contains(w.PermissionId))
                .Select(s => new AssignedPermissionModel
                {
                    RoleId = roleId,
                    PermissionAction = s.PermissionAction ?? "",
                    PermissionId = s.PermissionId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Removes all permission assignments for a specific security role.
        /// </summary>
        public async Task RemoveByRoleId(int roleId, int tenantId)
        {
            await EnsureCanEditRolePermissions(roleId, tenantId);

            var isSuperAdminRole = await _uow.SecurityRoleRepository.Query()
                .AnyAsync(sg => sg.RoleId == roleId
                                && sg.TenantId == tenantId
                                && sg.RoleName != null
                                && sg.RoleName == SuperAdminRoleName);
            if (isSuperAdminRole)
            {
                throw new InvalidOperationException("Super Admin role permissions cannot be removed.");
            }

            var itemsToRemove = await _uow.RolePermissionRepository.Query()
                .Where(x => x.RoleId == roleId && x.TenantId == tenantId)
                .ToListAsync();

            if (itemsToRemove.Count != 0)
            {
                _uow.RolePermissionRepository.RemoveRange(itemsToRemove);
                await _uow.CompleteAsync();
            }
        }

        private async Task EnsureCanEditRolePermissions(int roleId, int tenantId)
        {
            var targetRoleName = await _uow.SecurityRoleRepository.Query()
                .Where(sg => sg.RoleId == roleId && sg.TenantId == tenantId )
                .Select(sg => sg.RoleName ?? "")
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(targetRoleName))
            {
                throw new InvalidOperationException("Role not found.");
            }

            var currentUserId = _userContext.GetUserId();
            var currentUserRoleNames = await _userRoleService.GetRoleNamesForUser(currentUserId, tenantId);

            var currentRank = _userRoleService.GetHighestRank(currentUserRoleNames);
            var targetRank = _userRoleService.GetRank(targetRoleName);

            if (targetRank == Rank.SuperAdmin && currentRank != Rank.SuperAdmin)
            {
                throw new InvalidOperationException("Only Super Admin can edit permissions for the Super Admin role.");
            }

            if (targetRank == Rank.Admin && currentRank < Rank.Admin)
            {
                throw new InvalidOperationException("Only Admin or Super Admin can edit permissions for the Admin role.");
            }
        }
    }
}
