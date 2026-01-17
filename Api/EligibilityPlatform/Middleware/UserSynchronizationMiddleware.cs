using EligibilityPlatform.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

using EligibilityPlatform.Application.UnitOfWork;

namespace EligibilityPlatform.Middleware
{
public class UserSyncMiddleware(RequestDelegate next, IMemoryCache cache)
    {
    private readonly RequestDelegate _next = next;
    private readonly IMemoryCache _cache = cache;

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
        {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var sub = context.User.GetUserSubId();

        if (string.IsNullOrEmpty(sub))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"USER_SYNCED_{sub}";

        if (_cache.TryGetValue(cacheKey, out _))
        {
            await _next(context);
            return; 
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user =  db.UserRepository.Query().FirstOrDefault(u => u.KeycloakUserId == sub);

        if (user == null)
        {
            user = new User
            {
                KeycloakUserId = sub,
                UserName = context.User.GetUserName(),
                Email = context.User.GetUserEmail(),
                IsActive = true,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            db.UserRepository.Add(user);
        }
        else
        {
            var name = context.User.GetUserName();
            var email = context.User.GetUserEmail();

            if (user.UserName != name || user.Email != email)
            {
                user.UserName = name;
                user.Email = email;
            }
        }

        await db.CompleteAsync();

        _cache.Set(cacheKey, true, TimeSpan.FromMinutes(30));

        await _next(context);
    }
}

}
