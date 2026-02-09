using Microsoft.AspNetCore.Authorization;

namespace MEligibilityPlatform.Authorization
{
    public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}