using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Constants;
using EligibilityPlatform.Application.Services.Inteface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing validation operations for rules, E-cards, and P-cards.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ValidatorController"/> class.
    /// </remarks>
    /// <param name="validatorService">The validator service.</param>
    [Route("api/validator")]
    [ApiController]
    public class ValidatorController(IValidatorService validatorService) : ControllerBase
    {
        private readonly IValidatorService _validatorService = validatorService;

        /// <summary>
        /// Validates an e-rule by rule ID and key values.
        /// </summary>
        /// <param name="ruleId">The rule ID.</param>
        /// <param name="keyValues">The key values for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        /// 
        [Authorize(Policy = Permissions.Validator.Rule)]

        [HttpPost("validateerule")]
        public IActionResult ValidateRule(int ruleId, Dictionary<int, object> keyValues)
        {
            // Validates the e-rule using the validator service
            var result = _validatorService.ValidateRule(ruleId, keyValues);
            // Returns success response with the validation result
            return Ok(result);
        }

        /// <summary>
        /// Validates a form e-rule by expression and key values.
        /// </summary>
        /// <param name="expreesion">The expression to validate.</param>
        /// <param name="keyValues">The key values for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        /// 
        [Authorize(Policy = Permissions.Validator.Rule)]

        [HttpPost("validateformerule")]
        public IActionResult ValidateFormErule(string expreesion, Dictionary<int, object> keyValues)
        {
            // Validates the form e-rule using the validator service
            var result = _validatorService.ValidateFormErule(expreesion, keyValues);
            // Returns success response with the validation result
            return Ok(result);
        }

        /// <summary>
        /// Validates an ECard by ECard ID and key values.
        /// </summary>
        /// <param name="eCardId">The ECard ID.</param>
        /// <param name="keyValues">The key values for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        /// 
        [Authorize(Policy = Permissions.Validator.ECard)]

        [HttpPost("validateecard")]
        public async Task<IActionResult> ValidateECard(int eCardId, Dictionary<int, object> keyValues)
        {
            // Validates the ECard asynchronously using the validator service
            var result = await _validatorService.ValidateCard(eCardId, keyValues);
            // Returns success response with the validation result
            return Ok(result);
        }

        /// <summary>
        /// Validates a form ECard by expression and key values.
        /// </summary>
        /// <param name="expreesion">The expression to validate.</param>
        /// <param name="keyValues">The key values for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        /// 
        [Authorize(Policy = Permissions.Validator.ECard)]

        [HttpPost("validateformecard")]
        public IActionResult ValidateFormECard(string expreesion, Dictionary<int, object> keyValues)
        {
            // Validates the form ECard using the validator service
            var result = _validatorService.ValidateFormECard(expreesion, keyValues);
            // Returns success response with the validation result
            return Ok(result);
        }

        /// <summary>
        /// Validates a PCard by PCard ID and key values for the current user.
        /// </summary>
        /// <param name="pCardId">The PCard ID.</param>
        /// <param name="keyValues">The key values for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        /// 
        [Authorize(Policy = Permissions.Validator.PCard)]

        [HttpPost("validatepcard")]
        public async Task<IActionResult> ValidatePCards(int pCardId, Dictionary<int, object> keyValues)
        {
            // Validates the PCard asynchronously for the current user using the validator service
            var result = await _validatorService.ValidAsync(User.GetUserId(), pCardId, keyValues);
            // Returns success response with the validation result
            return Ok(result);
        }

        /// <summary>
        /// Validates a form PCard by expression and key values.
        /// </summary>
        /// <param name="expreesion">The expression to validate.</param>
        /// <param name="keyValues">The key values for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        /// 
        [Authorize(Policy = Permissions.Validator.PCard)]

        [HttpPost("validateformpcard")]
        public async Task<IActionResult> ValidateFormPCard(string expreesion, Dictionary<int, object> keyValues)
        {
            // Validates the form PCard asynchronously using the validator service
            var result = await _validatorService.ValidateFormPCard(expreesion, keyValues);
            // Returns success response with the validation result
            return Ok(result);
        }
    }
}
