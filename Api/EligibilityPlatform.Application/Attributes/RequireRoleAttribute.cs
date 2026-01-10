using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EligibilityPlatform.Application.Attributes
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireRoleAttribute(string role) : System.Attribute, IAuthorizationFilter
    {
        private readonly string _role = role;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Items["Roles"] is not List<string> roles || !roles.Contains(_role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new JsonResult(new
                {
                    message = "You do not have permission to perform this action."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}

