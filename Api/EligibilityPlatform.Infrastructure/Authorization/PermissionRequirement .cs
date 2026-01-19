using Microsoft.AspNetCore.Authorization;

namespace EligibilityPlatform.Infrastructure.Authorization
{
    public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}