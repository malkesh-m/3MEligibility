using MapsterMapper;
using MEligibilityPlatform.Application.Constants;
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

            // ── Step 1: Resolve dependency .View permissions ─────────────────
            // Get action strings for incoming permission IDs
            var incomingIds = rolePermissionModel.PermissionIds.ToHashSet();

            var incomingActions = await _uow.PermissionRepository.Query()
                .Where(p => incomingIds.Contains(p.PermissionId) && p.PermissionAction != null)
                .Select(p => p.PermissionAction!)
                .ToListAsync();

            // Expand with auto-dependencies (e.g. Factor.Create → Parameter.View)
            var expandedActions = PermissionDependencies.Resolve(incomingActions);

            // Map expanded action strings back to PermissionIds
            var expandedPermissionIds = await _uow.PermissionRepository.Query()
                .Where(p => p.PermissionAction != null && expandedActions.Contains(p.PermissionAction))
                .Select(p => p.PermissionId)
                .ToListAsync();

            // Merge original + expanded IDs
            var allPermissionIds = incomingIds.Union(expandedPermissionIds).ToHashSet();

            var existingPermissionIds = await _uow.RolePermissionRepository.Query()
                .Where(x => x.RoleId == rolePermissionModel.RoleId && x.TenantId == rolePermissionModel.TenantId)
                .Select(x => x.PermissionId)
                .ToListAsync();

            var rolePermissions = allPermissionIds
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

            // 1. Identify what the user explicitly wants to remove
            var incomingIds = rolePermissionModel.PermissionIds.ToHashSet();
            
            // Fetch relevant permissions in bulk to ensure mock reliability and performance
            var allPermissions = await _uow.PermissionRepository.Query()
                .Where(p => p.PermissionAction != null)
                .ToListAsync();

            var actionsToRemove = allPermissions
                .Where(p => incomingIds.Contains(p.PermissionId))
                .Select(p => p.PermissionAction!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (actionsToRemove.Count == 0) return;

            // Expand .Access removals to intelligently remove their underlying .Screens
            var accessActionsToRemove = actionsToRemove
                .Where(a => a.EndsWith(".Access", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            foreach (var accessAction in accessActionsToRemove)
            {
                actionsToRemove.UnionWith(PermissionDependencies.GetChildScreens(accessAction));
            }

            // Expand .Screen removals to intelligently remove their underlying functional actions (.Create, .Edit, etc.)
            // We ensure we only grab actions with the exact same prefix (e.g. "Rule." from "Rule.Screen")
            // .View and .Access are explicitly excluded from this sweep.
            var screenActionsToRemove = actionsToRemove
                .Where(a => a.EndsWith(".Screen", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var screenAction in screenActionsToRemove)
            {
                var prefix = screenAction[..(screenAction.LastIndexOf('.') + 1)]; // e.g. "Rule."
                
                var childActions = allPermissions
                    .Where(p => p.PermissionAction != null && 
                                p.PermissionAction.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && 
                                !p.PermissionAction.EndsWith(".View", StringComparison.OrdinalIgnoreCase) &&
                                !p.PermissionAction.EndsWith(".Access", StringComparison.OrdinalIgnoreCase) &&
                                !p.PermissionAction.EndsWith(".Screen", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.PermissionAction!);

                actionsToRemove.UnionWith(childActions);
            }

            // 2. Identify all permissions currently assigned to this role
            var assignedPermissions = await _uow.RolePermissionRepository.Query()
                .Where(w => w.RoleId == rolePermissionModel.RoleId && w.TenantId == rolePermissionModel.TenantId)
                .ToListAsync();

            if (assignedPermissions.Count == 0) return;

            var assignedPermissionIds = assignedPermissions.Select(rp => rp.PermissionId).ToHashSet();
            
            var permissionMap = allPermissions
                .Where(p => assignedPermissionIds.Contains(p.PermissionId))
                .ToDictionary(p => p.PermissionId, p => p.PermissionAction!);

            var assignedActions = permissionMap.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 3. Determine "What's Left" (Seeds only: technical .View and .Access items don't survive on their own to keep .Views alive)
            var leftActions = assignedActions
                .Where(a => !actionsToRemove.Contains(a))
                .Where(a => !a.EndsWith(".View", StringComparison.OrdinalIgnoreCase) && 
                            !a.EndsWith(".Access", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 4. Resolve "What's Left" to find survival set (Technical dependencies only)
            var actionsToKeep = PermissionDependencies.ResolveTechnicalOnly(leftActions);

            // 5. Final Removal Set: Explicitly requested removals + explicitly orphaned .View APIs.
            // We DO NOT auto-remove .Access, .Screen, or any other type. Those require explicit user deletion.
            var actionsToRemoveSet = new HashSet<string>(actionsToRemove, StringComparer.OrdinalIgnoreCase);
            
            var finalRemovalSet = new HashSet<string>(actionsToRemoveSet, StringComparer.OrdinalIgnoreCase);

            var orphanedViews = assignedActions
                .Where(a => a.EndsWith(".View", StringComparison.OrdinalIgnoreCase) && !actionsToKeep.Contains(a));

            finalRemovalSet.UnionWith(orphanedViews);

            // 6. Execute removal
            var itemsToRemove = assignedPermissions
                .Where(rp => permissionMap.TryGetValue(rp.PermissionId, out var action) && finalRemovalSet.Contains(action))
                .ToList();

            if (itemsToRemove.Count != 0)
            {
                _uow.RolePermissionRepository.RemoveRange(itemsToRemove);
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
        /// Retrieves permissions assigned to the specified role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>
        /// A list of <see cref="AssignedPermissionModel"/> representing assigned permissions.
        /// </returns>
        public async Task<IList<AssignedPermissionModel>> GetAssignedPermissions(int roleId, int tenantId)
        {
            var assigned = await _uow.RolePermissionRepository.Query()
                .Include(i => i.Permission)
                .Where(w => w.RoleId == roleId && w.TenantId == tenantId)
                // HIDE technical .view permissions; SHOW .Screen and .Access (Module Headers)
                .Where(w => w.Permission.PermissionAction != null && 
                           !w.Permission.PermissionAction.ToLower().Trim().EndsWith(".view"))
                .ToListAsync();

            // Group by Action to ensure UI only shows unique permission names
            return assigned
                .GroupBy(g => g.Permission.PermissionAction, StringComparer.OrdinalIgnoreCase)
                .Select(s => s.First())
                .Select(s => new AssignedPermissionModel
                {
                    RoleId = s.RoleId,
                    PermissionAction = s.Permission.PermissionAction ?? "",
                    PermissionName = FormatPermissionName(s.Permission.PermissionAction ?? ""),
                    PermissionId = s.PermissionId,
                    IsMasterSwitch = (s.Permission.PermissionAction ?? "").ToLower().EndsWith(".access"),
                    ModuleName = GetModuleName(s.Permission.PermissionAction ?? ""),
                    ResourceName = GetResourceName(s.Permission.PermissionAction ?? "")
                })
                .ToList();
        }

        /// <summary>
        /// Retrieves permissions not currently assigned to the specified role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>
        /// A list of <see cref="AssignedPermissionModel"/> representing unassigned permissions.
        /// </returns>
        public async Task<IList<AssignedPermissionModel>> GetUnAssignedPermissions(int roleId, int tenantId)
        {
            // Get action strings that are ALREADY assigned to this role
            var assignedPermissionActions = await _uow.RolePermissionRepository.Query()
                .Where(w => w.RoleId == roleId && w.TenantId == tenantId)
                .Join(_uow.PermissionRepository.Query(),
                    rp => rp.PermissionId,
                    p => p.PermissionId,
                    (rp, p) => p.PermissionAction)
                .Where(a => a != null)
                .Distinct()
                .ToListAsync();

            // Get all permissions, filter out .view, and remove those whose ACTION is already assigned
            var unassignedPermissions = await _uow.PermissionRepository.Query()
                .Where(w => w.PermissionAction != null && !w.PermissionAction.ToLower().Trim().EndsWith(".view"))
                .Where(w => !assignedPermissionActions.Contains(w.PermissionAction!))
                .ToListAsync();

            // Ensure unique actions in the result set to avoid duplicate rows in UI
            return unassignedPermissions
                .GroupBy(g => g.PermissionAction, StringComparer.OrdinalIgnoreCase)
                .Select(s => s.First())
                .Select(s => new AssignedPermissionModel
                {
                    RoleId = roleId,
                    PermissionAction = s.PermissionAction ?? "",
                    PermissionName = FormatPermissionName(s.PermissionAction ?? ""),
                    PermissionId = s.PermissionId,
                    IsMasterSwitch = (s.PermissionAction ?? "").ToLower().EndsWith(".access"),
                    ModuleName = GetModuleName(s.PermissionAction ?? ""),
                    ResourceName = GetResourceName(s.PermissionAction ?? "")
                })
                .ToList();
        }

        private static string FormatPermissionName(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "";

            // Remove "Permissions." prefix
            var prefix = "Permissions.";
            var clean = action.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) 
                ? action[prefix.Length..] 
                : action;

            // Replace "." with " "
            clean = clean.Replace(".", " ");

            // Special handling for Master Switches
            if (clean.EndsWith(" Access", StringComparison.OrdinalIgnoreCase))
            {
                return $"[MODULE] {clean.Replace(" Access", "", StringComparison.OrdinalIgnoreCase)}";
            }

            return clean;
        }

        private static string GetModuleName(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "General";
            
            var prefix = "Permissions.";
            var withoutPrefix = action.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) 
                ? action[prefix.Length..] 
                : action;

            var parts = withoutPrefix.Split('.');
            
            // First part is the module (e.g. MasterData)
            return parts.Length > 0 ? parts[0] : "General";
        }

        private static string GetResourceName(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "General";
            
            var prefix = "Permissions.";
            var withoutPrefix = action.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) 
                ? action[prefix.Length..] 
                : action;

            var parts = withoutPrefix.Split('.');
            
            // If it's Permissions.Parameter.View -> Resource is Parameter
            // The mapping from Resource to Module is actually complex 
            // but for now we follow the parts[0] convention
            return parts.Length > 0 ? parts[0] : "General";
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
                .Where(sg => sg.RoleId == roleId && sg.TenantId == tenantId)
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
