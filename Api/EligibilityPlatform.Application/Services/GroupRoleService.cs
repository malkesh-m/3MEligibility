using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing group-role associations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GroupRoleService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class GroupRoleService(IUnitOfWork uow, IMapper mapper) : IGroupRoleService
    {
        /// <summary>
        /// The unit of work instance for data access and persistence operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The AutoMapper instance for mapping between entities and models.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds group-role assignments based on the given model.
        /// </summary>
        /// <param name="groupRoleModel">
        /// The model containing the group ID and list of roles to assign.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(GroupRoleModel groupRoleModel)
        {
            var groupRoles = groupRoleModel.RoleIds
                .Select(roleId => new GroupRole
                {
                    GroupId = groupRoleModel.GroupId,
                    RoleId = roleId,
                    UpdatedByDateTime = DateTime.UtcNow
                })
                .ToList();

            // Add all mappings at once
            _uow.GroupRoleRepository.AddRange(groupRoles);

            // Commit changes
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all group-role mappings.
        /// </summary>
        /// <returns>A list of <see cref="GroupRoleModel"/> instances.</returns>
        public List<GroupRoleModel> GetAll()
        {
            // Retrieve all groupRole entities.
            var groupRoles = _uow.GroupRoleRepository.GetAll();
            // Map entities to models using AutoMapper.
            return _mapper.Map<List<GroupRoleModel>>(groupRoles);
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
            return await _uow.GroupRoleRepository.GetBySecurityGroupId(groupId);
        }

        /// <summary>
        /// Removes specified roles from a given group.
        /// </summary>
        /// <param name="groupRoleModel">
        /// The model containing the group ID and list of roles to remove.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(GroupRoleModel groupRoleModel)
        {
            var itemsToRemove = new List<GroupRole>();

            // Collect all mappings to remove
            foreach (var roleId in groupRoleModel.RoleIds)
            {
                var item = await _uow.GroupRoleRepository
                    .GetGroupRole(groupRoleModel.GroupId, roleId);

                if (item != null)
                {
                    itemsToRemove.Add(item);
                }
            }

            // Remove all at once
            if (itemsToRemove.Count != 0)
            {
                _uow.GroupRoleRepository.RemoveRange(itemsToRemove);
            }

            // Commit changes
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves roles assigned to the specified group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <returns>
        /// A list of <see cref="AssignedRoleModel"/> representing assigned roles.
        /// </returns>
        public async Task<IList<AssignedRoleModel>> GetAssignedRoles(int groupId)
        {
            // Query existing mappings including related Role entity.
            return await _uow.GroupRoleRepository.Query()
                .Include(i => i.Role)
                .Where(w => w.GroupId == groupId)
                .Select(s => new AssignedRoleModel
                {
                    GroupId = s.GroupId,
                    RoleAction = s.Role.RoleAction ?? "",
                    RoleId = s.RoleId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves roles not currently assigned to the specified group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <returns>
        /// A list of <see cref="AssignedRoleModel"/> representing unassigned roles.
        /// </returns>
        public async Task<IList<AssignedRoleModel>> GetUnAssignedRoles(int groupId)
        {
            // Retrieve all roles.
            var roles = _uow.RoleRepository.GetAll();
            // Identify IDs already assigned to the group.
            var assignedRoleIds = _uow.GroupRoleRepository.Query()
                .Include(i => i.Role)
                .Where(w => w.GroupId == groupId)
                .Select(s => s.RoleId);

            // Query roles that are not in assignedRoleIds.
            return await _uow.RoleRepository.Query()
                .Where(w => !assignedRoleIds.Contains(w.RoleId))
                .Select(s => new AssignedRoleModel
                {
                    GroupId = groupId,
                    RoleAction = s.RoleAction ?? "",
                    RoleId = s.RoleId
                })
                .ToListAsync();
        }
    }
}
