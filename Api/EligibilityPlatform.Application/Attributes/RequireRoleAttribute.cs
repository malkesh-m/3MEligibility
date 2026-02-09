using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
namespace MEligibilityPlatform.Application.Attributes
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequirePermissionAttribute(string permission) : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission = permission;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Assume user ID is in Claims
            var userIdClaim = context.HttpContext.User.FindFirst("user_id")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = 401 };
                return;
            }

            // Resolve service
            var service = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            var permissions = await service.GetUserPermissionsAsync(userId);

            if (!permissions.Contains(_permission))
            {
                context.Result = new JsonResult(new { message = "You do not have permission to perform this action." })
                { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}