using System.Runtime.CompilerServices;
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
    /// Service class for managing group-role associations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GroupPermissionService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class GroupPermissionService(IUnitOfWork uow, IMapper mapper,IMemoryCache cache,IUserService userService, IUserContextService userContext,IUserGroupService userGroupService) : IGroupPermissionService
    {
        private const string SuperAdminGroupName = "Super Admin";

        /// <summary>
        /// The unit of work instance for data access and persistence operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;
        private readonly IUserService _userService = userService;
        private readonly IUserContextService _userContext = userContext;
        /// <summary>
        /// The AutoMapper instance for mapping between entities and models.
        /// </summary>
        private readonly IMapper _mapper = mapper;
        private readonly IMemoryCache _cache = cache;
        private readonly IUserGroupService _userGroupService = userGroupService;
        /// <summary>
        /// Adds group-role assignments based on the given model.
        /// </summary>
        /// <param name="groupPermissionModel">
        /// The model containing the group ID and list of roles to assign.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(GroupPermissionModel groupPermissionModel)
        {
            await EnsureCanEditGroupPermissions(groupPermissionModel.GroupId, groupPermissionModel.TenantId);

            var isSuperAdminGroup = await _uow.SecurityGroupRepository.Query()
                .AnyAsync(sg => sg.GroupId == groupPermissionModel.GroupId
                                && sg.TenantId == groupPermissionModel.TenantId
                                && sg.GroupName != null
                                && sg.GroupName == SuperAdminGroupName);
            // Super Admin can add permissions to Super Admin group (removal still blocked).

            var existingPermissionIds = await _uow.GroupPermissionRepository.Query()
                .Where(x => x.GroupId == groupPermissionModel.GroupId && x.TenantId == groupPermissionModel.TenantId)
                .Select(x => x.PermissionId)
                .ToListAsync();

            var groupRoles = groupPermissionModel.PermissionIds
                .Where(roleId => !existingPermissionIds.Contains(roleId))
                .Select(roleId => new GroupPermission
                {
                    GroupId = groupPermissionModel.GroupId,
                    PermissionId = roleId,
                    TenantId = groupPermissionModel.TenantId,
                    UpdatedByDateTime = DateTime.UtcNow
                })
                .ToList();
            if (groupRoles.Count != 0)
            {
                // Add all mappings at once
                _uow.GroupPermissionRepository.AddRange(groupRoles);
                // Commit changes
                await _uow.CompleteAsync();
            }

            var userIds = await _uow.UserGroupRepository.Query()
              .Where(x => x.GroupId == groupPermissionModel.GroupId)
             .Select(x => x.UserId)
             .ToListAsync();
            foreach (var userId in userIds)
            {
                _userService.RemoveUserPermissionsCache(userId);
            }
        }

        /// <summary>
        /// Retrieves all group-role mappings.
        /// </summary>
        /// <returns>A list of <see cref="GroupPermissionModel"/> instances.</returns>
        public List<GroupPermissionModel> GetAll()
        {
            // Retrieve all groupRole entities.
            var groupRoles = _uow.GroupPermissionRepository.GetAll();
            // Map entities to models using AutoMapper.
            return _mapper.Map<List<GroupPermissionModel>>(groupRoles);
        }

        /// <summary>
        /// Determines whether any roles are associated with the given security group ID.
        /// </summary>
        /// <param name="groupId">The ID of the group to check.</param>
        /// <returns>
        /// True if at least one role is associated; otherwise, false.
        /// </returns>
        public async Task<bool> GetBySecurityGroupId(int groupId)
        {
            // Check existence in repository.
            return await _uow.GroupPermissionRepository.GetBySecurityGroupId(groupId);
        }

        /// <summary>
        /// Removes specified roles from a given group.
        /// </summary>
        /// <param name="groupPermissionModel">
        /// The model containing the group ID and list of roles to remove.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(GroupPermissionModel groupPermissionModel)
        {
           
            await EnsureCanEditGroupPermissions(groupPermissionModel.GroupId, groupPermissionModel.TenantId);

            var isSuperAdminGroup = await _uow.SecurityGroupRepository.Query()
                .AnyAsync(sg => sg.GroupId == groupPermissionModel.GroupId
                                && sg.TenantId == groupPermissionModel.TenantId
                                && sg.GroupName != null
                                && sg.GroupName == SuperAdminGroupName);
            if (isSuperAdminGroup)
            {
                throw new InvalidOperationException("Super Admin group permissions cannot be removed.");
            }

            var itemsToRemove = new List<GroupPermission>();

            // Collect all mappings to remove
            foreach (var roleId in groupPermissionModel.PermissionIds)
            {
                var item = await _uow.GroupPermissionRepository
                    .GetGroupPermission(groupPermissionModel.GroupId, roleId);

                if (item != null)
                {
                    itemsToRemove.Add(item);
                }
            }

            // Remove all at once
            if (itemsToRemove.Count != 0)
            {
                _uow.GroupPermissionRepository.RemoveRange(itemsToRemove);
            }
            // Commit changes
            await _uow.CompleteAsync();
            var userIds = await _uow.UserGroupRepository.Query()
            .Where(x => x.GroupId == groupPermissionModel.GroupId)
            .Select(x => x.UserId)
            .ToListAsync();
            foreach (var userId in userIds)
            {
                _userService.RemoveUserPermissionsCache(userId);
            }
        }

        /// <summary>
        /// Retrieves roles assigned to the specified group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <returns>
        /// A list of <see cref="AssignedPermissionModel"/> representing assigned roles.
        /// </returns>
        public async Task<IList<AssignedPermissionModel>> GetAssignedPermissions(int groupId)
        {
            // Query existing mappings including related Permission entity.
            return await _uow.GroupPermissionRepository.Query()
                .Include(i => i.Permission)
                .Where(w => w.GroupId == groupId)
                .Select(s => new AssignedPermissionModel
                {
                    GroupId = s.GroupId,
                    PermissionAction = s.Permission.PermissionAction ?? "",
                    PermissionId = s.PermissionId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves roles not currently assigned to the specified group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <returns>
        /// A list of <see cref="AssignedPermissionModel"/> representing unassigned roles.
        /// </returns>
        public async Task<IList<AssignedPermissionModel>> GetUnAssignedPermissions(int groupId)
        {
            // Retrieve all roles.
            var roles = _uow.PermissionRepository.GetAll();
            // Identify IDs already assigned to the group.
            var assignedPermissionIds = _uow.GroupPermissionRepository.Query()
                .Include(i => i.Permission)
                .Where(w => w.GroupId == groupId)
                .Select(s => s.PermissionId);

            // Query roles that are not in assignedPermissionIds.
            return await _uow.PermissionRepository.Query()
                .Where(w => !assignedPermissionIds.Contains(w.PermissionId))
                .Select(s => new AssignedPermissionModel
                {
                    GroupId = groupId,
                    PermissionAction = s.PermissionAction ?? "",
                    PermissionId = s.PermissionId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Removes all role assignments for a specific security group.
        /// </summary>
        public async Task RemoveByGroupId(int groupId, int tenantId)
        {
            await EnsureCanEditGroupPermissions(groupId, tenantId);

            var isSuperAdminGroup = await _uow.SecurityGroupRepository.Query()
                .AnyAsync(sg => sg.GroupId == groupId
                                && sg.TenantId == tenantId
                                && sg.GroupName != null
                                && sg.GroupName == SuperAdminGroupName);
            if (isSuperAdminGroup)
            {
                throw new InvalidOperationException("Super Admin group permissions cannot be removed.");
            }

            var itemsToRemove = await _uow.GroupPermissionRepository.Query()
                .Where(x => x.GroupId == groupId && x.TenantId == tenantId)
                .ToListAsync();

            if (itemsToRemove.Count != 0)
            {
                _uow.GroupPermissionRepository.RemoveRange(itemsToRemove);
                await _uow.CompleteAsync();
            }
        }

        private async Task EnsureCanEditGroupPermissions(int groupId, int tenantId)
        {
            var targetGroupName = await _uow.SecurityGroupRepository.Query()
                .Where(sg => sg.GroupId == groupId && (sg.TenantId == tenantId || sg.TenantId == 0))
                .Select(sg => sg.GroupName ?? "")
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(targetGroupName))
            {
                throw new InvalidOperationException("Group not found.");
            }

            var currentUserId = _userContext.GetUserId();
            var currentUserGroupNames = await (from ug in _uow.UserGroupRepository.Query()
                                               join sg in _uow.SecurityGroupRepository.Query()
                                                   on ug.GroupId equals sg.GroupId
                                               where ug.UserId == currentUserId
                                                     && (ug.TenantId == tenantId || ug.TenantId == 0)
                                                     && sg.TenantId == tenantId
                                               select sg.GroupName ?? "")
                .Distinct()
                .ToListAsync();

            var currentRank = _userGroupService.GetHighestRank(currentUserGroupNames);
            var targetRank = _userGroupService.GetRank(targetGroupName);

            if (targetRank == Rank.SuperAdmin && currentRank != Rank.SuperAdmin)
            {
                throw new InvalidOperationException("Only Super Admin can edit permissions for the Super Admin group.");
            }

            if (targetRank == Rank.Admin && currentRank < Rank.Admin)
            {
                throw new InvalidOperationException("Only Admin or Super Admin can edit permissions for the Admin group.");
            }
        }

   

    }
}

