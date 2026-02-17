using Microsoft.AspNetCore.Authorization;
using MEligibilityPlatform.Application.Services.Interface;
using System.Security.Claims;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Authorization
{
    public sealed class PermissionAuthorizationHandler(IUserService userService)
                : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserService _userService = userService;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return; 
            var userIdClaim = context.User.GetUserId(); ;
            var tenantId = context.User.GetTenantId();
            var permissions = await _userService.GetUserPermissionsAsync(userIdClaim,tenantId);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}