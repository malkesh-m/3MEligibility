using Microsoft.AspNetCore.Authorization;
using EligibilityPlatform.Application.Services.Inteface;
using System.Security.Claims;

namespace EligibilityPlatform.Infrastructure.Authorization
{
    public sealed class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserService _userService;

        public PermissionAuthorizationHandler(IUserService userService)
        {
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userIdClaim = context.User.Claims
                   .FirstOrDefault(c =>
                       c.Type.Equals("user_id", StringComparison.OrdinalIgnoreCase))
                   ?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return;

            var permissions = await _userService.GetUserPermissionsAsync(userId);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}