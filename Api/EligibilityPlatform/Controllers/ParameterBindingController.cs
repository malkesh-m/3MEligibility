using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParameterBindingController(IParameterBindingService bindingService, ILogger<ParameterBindingController> logger) : ControllerBase
    {
        private readonly IParameterBindingService _bindingService = bindingService;
        private readonly ILogger<ParameterBindingController> _logger = logger;
        [Authorize(Policy =Permissions.ParameterBinding.View)]
        [HttpGet]
        public  IActionResult Get()
        {
            var tenantId = User.GetTenantId();
            var result =  _bindingService.GetAllBindings(tenantId);
            return Ok(result);
        }
        [Authorize(Policy = Permissions.ParameterBinding.Create)]

        [HttpPost]
        public async Task<IActionResult> Post( ParameterBindingAddModel model)
        {
            try
            {
                var tenantId = User.GetTenantId();
                await _bindingService.SaveBinding(tenantId, model);
                return Ok(new { IsSuccess = true, Message = "Binding saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving binding");
                return BadRequest(new { IsSuccess = false, ex.Message });
            }
        }
    }


}
