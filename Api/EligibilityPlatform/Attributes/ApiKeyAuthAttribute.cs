using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MEligibilityPlatform.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "x-api-key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var validApiKey = config["ApiKeySettings:ValidApiKey"];

            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new JsonResult(new
                {
                    message = "API Key is missing",
                    code = "API_KEY_MISSING"
                })
                {
                    StatusCode = StatusCodes.Status402PaymentRequired //  custom code (won’t trigger logout)
                };
                return;
            }

            if (!string.Equals(extractedApiKey.ToString(), validApiKey, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new JsonResult(new
                {
                    message = "Invalid API Key",
                    code = "API_KEY_INVALID"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden //  use 403 instead of 401
                };
                return;
            }

            await next();
        }
    }
}
