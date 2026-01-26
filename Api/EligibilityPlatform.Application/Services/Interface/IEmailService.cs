using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for email operations.
    /// Provides methods for sending various types of emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a password reset email to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="resetLink">The password reset link to include in the email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendPasswordResetEmailAsync(string email, string resetLink);

        /// <summary>
        /// Sends a generic email based on the provided mail request.
        /// </summary>
        /// <param name="mailRequest">The <see cref="MailRequest"/> containing email details.</param>
        /// <returns>A task representing the asynchronous operation.
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
