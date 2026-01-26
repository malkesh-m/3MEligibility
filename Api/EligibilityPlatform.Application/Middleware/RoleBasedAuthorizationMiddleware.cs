using System.Net.Http;
using System.Security.Claims;
using MEligibilityPlatform.Application.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace MEligibilityPlatform.Application.Middleware
{
    public class RoleBasedAuthorizationMiddleware(RequestDelegate next, IMemoryCache cache, IServiceScopeFactory scopeFactory)
    {
        private readonly IMemoryCache _cache = cache;

        private readonly RequestDelegate _next = next;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;


        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = context.User;
            var cacheKey = $"USER_ROLES_{userId}";

            // Try get roles from cache
            if (!_cache.TryGetValue(cacheKey, out List<string?>? roles))
            {
                // Fetch from DB only if authenticated
                if (user.Identity?.IsAuthenticated == true)
                {
                    var groupId = user.FindFirst(ClaimTypes.Role)?.Value;

                    if (!string.IsNullOrEmpty(groupId))
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        roles = await db.GroupRoleRepository.Query()
                            .Include(gr => gr.Role)
                            .Where(gr => gr.GroupId.ToString() == groupId)
                            .Select(gr => gr.Role.RoleAction)
                            .ToListAsync();

                        _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(10));
                    }
                }
            }

            if (roles != null)
            {
                context.Items["Roles"] = roles;
            }

            await _next(context);
        }
    }
}
