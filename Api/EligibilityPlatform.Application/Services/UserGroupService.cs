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
    /// Service class for managing user groups.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserGroupService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class UserGroupService(IUnitOfWork uow, IMapper mapper,IUserService userService) : IUserGroupService
    {
        private const string SuperAdminGroupName = "Super Admin";
        private const string AdminGroupName = "Admin";
        private const string UserGroupName = "User";
        /// The unit of work instance for database operations.
        private readonly IUnitOfWork _uow = uow;

        /// The AutoMapper instance for object mapping.
        private readonly IMapper _mapper = mapper;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Adds a new user group to the database.
        /// </summary>
        /// <param name="userGroupModel">The UserGroupModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<string> Add(UserGroupCreateUpdateModel userGroupModel)
        {
            string result = "";
            var alreadyExist = _uow.UserGroupRepository.Query().Any(u => u.UserId == userGroupModel.UserId && u.GroupId == userGroupModel.GroupId && u.TenantId == userGroupModel.TenantId);
            if (alreadyExist)
            {
                result = "User already added in this group";
                return result;
            }
            userGroupModel.CreatedByDateTime = DateTime.UtcNow;
            // Sets the update timestamp to current UTC time
            userGroupModel.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to UserGroup entity and adds to repository
            _uow.UserGroupRepository.Add(_mapper.Map<UserGroup>(userGroupModel));
            // Commits the changes to the database
            await _uow.CompleteAsync();
            _userService.RemoveUserPermissionsCache(userGroupModel.UserId);

            result = "Success";
            return result;
        }

        /// <summary>
        /// Gets all user groups.
        /// </summary>
        /// <returns>A list of UserGroupModel representing all user groups.</returns>
        public List<UserGroupModel> GetAll()
        {
            // Retrieves all user groups from repository
            var userGroups = _uow.UserGroupRepository.GetAll();
            // Maps the user groups to models and returns
            return _mapper.Map<List<UserGroupModel>>(userGroups);
        }

        /// <summary>
        /// Gets users by group ID.
        /// </summary>
        /// <param name="id">The group ID.</param>
        /// <returns>A list of UserInfo for the specified group ID.</returns>
        public List<UserInfo> GetUserByGroupId(int id, ApiResponse<List<UserGetModel>> users)
        {
            // Retrieves users belonging to the specified group ID
            var userGroups = _uow.UserGroupRepository.GetUserByGroupId(id,users);
            // Returns the list of user information
            return userGroups;
        }

        /// <summary>
        /// Checks if a user group exists by group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence.</returns>
        public async Task<bool> GetByUserGroupId(int groupId)
        {
            // Checks if user group exists by group ID
            return await _uow.UserGroupRepository.GetByUserGroupId(groupId);
        }

        /// <summary>
        /// Checks if a user group exists by group ID (alternative method).
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence.</returns>
        public async Task<bool> GetByUserGroupsId(int groupId)
        {
            // Alternative method to check if user group exists by group ID
            return await _uow.UserGroupRepository.GetByUserGroupsId(groupId);
        }

        /// <summary>
        /// Gets the number of users assigned to a group within a tenant.
        /// </summary>
        public async Task<int> GetUserCountByGroupId(int groupId, int tenantId)
        {
            return await _uow.UserGroupRepository.Query()
                .Where(ug => ug.GroupId == groupId && (ug.TenantId == tenantId || ug.TenantId == 0))
                .CountAsync();
        }

        /// <summary>
        /// Gets the number of groups a user belongs to within a tenant.
        /// </summary>
        public async Task<int> GetGroupCountByUserId(int userId, int tenantId)
        {
            return await (from ug in _uow.UserGroupRepository.Query()
                          join sg in _uow.SecurityGroupRepository.Query()
                              on ug.GroupId equals sg.GroupId
                          where ug.UserId == userId
                                && sg.TenantId == tenantId
                          select ug.GroupId)
                .Distinct()
                .CountAsync();
        }

        /// <summary>
        /// Retrieves group names for a specific user within a tenant.
        /// </summary>
        public async Task<List<string>> GetGroupNamesForUser(int userId, int tenantId)
        {
            return await (from ug in _uow.UserGroupRepository.Query()
                          join sg in _uow.SecurityGroupRepository.Query()
                              on ug.GroupId equals sg.GroupId
                          where ug.UserId == userId && sg.TenantId == tenantId
                          select sg.GroupName ?? "")
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a group name by group ID within a tenant.
        /// </summary>
        public async Task<string?> GetGroupNameById(int groupId, int tenantId)
        {
            return await _uow.SecurityGroupRepository.Query()
                .Where(sg => sg.GroupId == groupId && (sg.TenantId == tenantId || sg.TenantId == 0))
                .Select(sg => sg.GroupName)
                .FirstOrDefaultAsync();
        }
        public async Task<Dictionary<int, string>> GetGroupNamesByIds(
            List<int> groupIds,
            int tenantId)
        {
            return await _uow.SecurityGroupRepository.Query()
                .Where(sg => groupIds.Contains(sg.GroupId) &&
                             (sg.TenantId == tenantId || sg.TenantId == 0))
                .ToDictionaryAsync(sg => sg.GroupId, sg => sg.GroupName ?? "");
        }
        /// <summary>
        /// Removes a user group by its ID.
        /// </summary>
        /// <param name="id">The user group ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieves the user group entity by ID
            var item = _uow.UserGroupRepository.GetById(id);
            // Removes the user group from repository
            _uow.UserGroupRepository.Remove(item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
        public  Rank GetRank(string groupName)
        {
            if (groupName.Equals(SuperAdminGroupName, StringComparison.OrdinalIgnoreCase))
            {
                return Rank.SuperAdmin;
            }
            if (groupName.Equals(AdminGroupName, StringComparison.OrdinalIgnoreCase))
            {
                return Rank.Admin;
            }
            if (groupName.Equals(UserGroupName, StringComparison.OrdinalIgnoreCase))
            {
                return Rank.User;
            }
            return Rank.None;
        }
        public Rank GetHighestRank(IEnumerable<string> groupNames)
        {
            var highest = Rank.None;
            foreach (var groupName in groupNames)
            {
                    var rank = GetRank(groupName);
                    ;
                if (rank > highest)
                {
                    highest = rank;
                }
            }
            return highest;
        }

        /// <summary>
        /// Removes a user from a group by user ID and group ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="groupId">The group ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveUserGroup(int userId, int groupId)
        {
            // Attempts to delete the user-group relationship
            var item = await _uow.UserGroupRepository.DeleteUserGroupAsync(userId, groupId);

            if (item != null)
            {
                // Removes the user group relationship from repository
                _uow.UserGroupRepository.Remove(item);
                // Commits the changes to the database
                await _uow.CompleteAsync();
                _userService.RemoveUserPermissionsCache(userId);
            }
            else
            {
                // Throws exception if user-group relationship not found
                throw new Exception("UserGroup not found.");
            }
        }
    }
}
