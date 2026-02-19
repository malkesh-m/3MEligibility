using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing user roles.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserRoleService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The Mapster mapper instance.</param>
    public class UserRoleService(IUnitOfWork uow, IMapper mapper, IUserService userService) : IUserRoleService
    {
        private const string SuperAdminRoleName = "Super Admin";
        private const string AdminRoleName = "Admin";
        private const string UserRoleName = "User";
        
        /// The unit of work instance for database operations.
        private readonly IUnitOfWork _uow = uow;

        /// The Mapster mapper instance for object mapping.
        private readonly IMapper _mapper = mapper;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Adds a new user role to the database.
        /// </summary>
        /// <param name="userRoleModel">The UserRoleModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<string> Add(UserRoleCreateUpdateModel userRoleModel)
        {
            string result = "";
            var alreadyExist = _uow.UserRoleRepository.Query().Any(u => u.UserId == userRoleModel.UserId && u.RoleId == userRoleModel.RoleId && u.TenantId == userRoleModel.TenantId);
            if (alreadyExist)
            {
                result = "User already added in this role";
                return result;
            }
            userRoleModel.CreatedByDateTime = DateTime.UtcNow;
            userRoleModel.UpdatedByDateTime = DateTime.UtcNow;
            _uow.UserRoleRepository.Add(_mapper.Map<UserRole>(userRoleModel));
            await _uow.CompleteAsync();
            _userService.RemoveUserPermissionsCache(userRoleModel.UserId);

            result = "Success";
            return result;
        }

        /// <summary>
        /// Gets all user roles.
        /// </summary>
        /// <returns>A list of UserRoleModel representing all user roles.</returns>
        public List<UserRoleModel> GetAll()
        {
            var userRoles = _uow.UserRoleRepository.GetAll();
            return _mapper.Map<List<UserRoleModel>>(userRoles);
        }

        /// <summary>
        /// Gets users by role ID.
        /// </summary>
        /// <param name="id">The role ID.</param>
        /// <returns>A list of UserInfo for the specified role ID.</returns>
        public List<UserInfo> GetUserByRoleId(int id, ApiResponse<List<UserGetModel>> users)
        {
            var userRoles = _uow.UserRoleRepository.GetUserByRoleId(id, users);
            return userRoles;
        }

        /// <summary>
        /// Checks if a user role exists by role ID.
        /// </summary>
        /// <param name="roleId">The role ID.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence.</returns>
        public async Task<bool> GetByUserRoleId(int roleId)
        {
            return await _uow.UserRoleRepository.GetByUserRoleId(roleId);
        }

        /// <summary>
        /// Checks if a user role exists by role ID (alternative method).
        /// </summary>
        /// <param name="roleId">The role ID.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence.</returns>
        public async Task<bool> GetByUserRolesId(int roleId)
        {
            return await _uow.UserRoleRepository.GetByUserRolesId(roleId);
        }

        /// <summary>
        /// Gets the number of users assigned to a role within a tenant.
        /// </summary>
        public async Task<int> GetUserCountByRoleId(int roleId, int tenantId)
        {
            return await _uow.UserRoleRepository.Query()
                .Where(ug => ug.RoleId == roleId && (ug.TenantId == tenantId || ug.TenantId == 0))
                .CountAsync();
        }

        /// <summary>
        /// Gets the number of roles a user belongs to within a tenant.
        /// </summary>
        public async Task<int> GetRoleCountByUserId(int userId, int tenantId)
        {
            return await (from ur in _uow.UserRoleRepository.Query()
                          join sr in _uow.SecurityRoleRepository.Query()
                              on ur.RoleId equals sr.RoleId
                          where ur.UserId == userId
                                && sr.TenantId == tenantId
                          select ur.RoleId)
                .Distinct()
                .CountAsync();
        }

        /// <summary>
        /// Retrieves role names for a specific user within a tenant.
        /// </summary>
        public async Task<List<string>> GetRoleNamesForUser(int userId, int tenantId)
        {
            return await (from ur in _uow.UserRoleRepository.Query()
                          join sr in _uow.SecurityRoleRepository.Query()
                              on ur.RoleId equals sr.RoleId
                          where ur.UserId == userId && sr.TenantId == tenantId
                          select sr.RoleName ?? "")
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a role name by role ID within a tenant.
        /// </summary>
        public async Task<string?> GetRoleNameById(int roleId, int tenantId)
        {
            return await _uow.SecurityRoleRepository.Query()
                .Where(sr => sr.RoleId == roleId && (sr.TenantId == tenantId || sr.TenantId == 0))
                .Select(sr => sr.RoleName)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, string>> GetRoleNamesByIds(
            List<int> roleIds,
            int tenantId)
        {
            return await _uow.SecurityRoleRepository.Query()
                .Where(sr => roleIds.Contains(sr.RoleId) &&
                             (sr.TenantId == tenantId || sr.TenantId == 0))
                .ToDictionaryAsync(sr => sr.RoleId, sr => sr.RoleName ?? "");
        }

        /// <summary>
        /// Removes a user role by its ID.
        /// </summary>
        /// <param name="id">The user role ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            var item = _uow.UserRoleRepository.GetById(id);
            _uow.UserRoleRepository.Remove(item);
            await _uow.CompleteAsync();
        }

        public Rank GetRank(string roleName)
        {
            if (roleName.Equals(SuperAdminRoleName, StringComparison.OrdinalIgnoreCase))
            {
                return Rank.SuperAdmin;
            }
            if (roleName.Equals(AdminRoleName, StringComparison.OrdinalIgnoreCase))
            {
                return Rank.Admin;
            }
            if (roleName.Equals(UserRoleName, StringComparison.OrdinalIgnoreCase))
            {
                return Rank.User;
            }
            return Rank.None;
        }

        public Rank GetHighestRank(IEnumerable<string> roleNames)
        {
            var highest = Rank.None;
            foreach (var roleName in roleNames)
            {
                var rank = GetRank(roleName);
                if (rank > highest)
                {
                    highest = rank;
                }
            }
            return highest;
        }

        /// <summary>
        /// Removes a user from a role by user ID and role ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="roleId">The role ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveUserRole(int userId, int roleId)
        {
            var item = await _uow.UserRoleRepository.DeleteUserRoleAsync(userId, roleId);

            if (item != null)
            {
                _uow.UserRoleRepository.Remove(item);
                await _uow.CompleteAsync();
                _userService.RemoveUserPermissionsCache(userId);
            }
            else
            {
                throw new Exception("UserRole not found.");
            }
        }
    }
}
