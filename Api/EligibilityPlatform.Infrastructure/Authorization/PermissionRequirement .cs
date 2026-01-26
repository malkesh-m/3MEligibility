using Microsoft.AspNetCore.Authorization;

namespace MEligibilityPlatform.Infrastructure.Authorization
{
    public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}