using System.ComponentModel.DataAnnotations;
using MEligibilityPlatform.Domain.Models;
using ValidationResult = MEligibilityPlatform.Domain.Models.ValidationResult;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for validation operations.
    /// Provides methods for validating various entities and rules against provided data.
    /// </summary>
    public interface IValidatorService
    {
        /// <summary>
        /// Validates a user against a specific card with provided key-value pairs.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to validate.</param>
        /// <param name="pCardId">The unique identifier of the card to validate against.</param>
        /// <param name="keyValues">The dictionary of key-value pairs containing validation data.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="ValidationResult"/> of the validation.</returns>
        Task<ValidationResult> ValidAsync(int userId, int pCardId, Dictionary<int, object> keyValues);

        /// <summary>
        /// Validates a form expression against a card with provided key-value pairs.
        /// </summary>
        /// <param name="expreesion">The expression string to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs containing validation data.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="ValidationResult"/> of the validation.</returns>
        Task<ValidationResult> ValidateFormPCard(string expreesion, Dictionary<int, object> keyValues);

        /// <summary>
        /// Validates a specific rule against provided key-value pairs.
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs containing validation data.</param>
        /// <returns>The <see cref="ValidationResult"/> of the rule validation.</returns>
        ValidationResult ValidateRule(int ruleId, Dictionary<int, object> keyValues);

        /// <summary>
        /// Validates a form expression against a rule with provided key-value pairs.
        /// </summary>
        /// <param name="expreesion">The expression string to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs containing validation data.</param>
        /// <returns>The <see cref="ValidationResult"/> of the expression validation.</returns>
        ValidationResult ValidateFormErule(string expreesion, Dictionary<int, object> keyValues);

        /// <summary>
        /// Validates a specific card with provided key-value pairs.
        /// </summary>
        /// <param name="eCardId">The unique identifier of the card to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs containing validation data.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="ValidationResult"/> of the card validation.</returns>
        Task<ValidationResult> ValidateCard(int eCardId, Dictionary<int, object> keyValues);

        /// <summary>
        /// Validates a form expression against a card with provided key-value pairs.
        /// </summary>
        /// <param name="expreesion">The expression string to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs containing validation data.</param>
        /// <returns>The <see cref="ValidationResult"/> of the expression validation.</returns>
        ValidationResult ValidateFormECard(string expreesion, Dictionary<int, object> keyValues);
    }
}
