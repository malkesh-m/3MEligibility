using System.Net;
using System.Net.Mail;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Domain.Models;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides email-related services, including sending password reset emails and general email notifications.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </remarks>
    /// <param name="mailSettings">The mail settings options.</param>
    public class EmailService(IOptions<MailSettings> mailSettings) : IEmailService
    {
        /// <summary>
        /// The mail settings configuration containing email server details.
        /// </summary>
        private readonly MailSettings _mailSettings = mailSettings.Value;

        /// <summary>
        /// Sends a password reset email asynchronously to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="resetLink">The password reset link.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            try
            {
                // Creates a new SMTP client instance with the configured mail server
                var smtpClient = new System.Net.Mail.SmtpClient(_mailSettings.Mail)
                {
                    // Sets the port number for the SMTP server
                    Port = _mailSettings.Port,
                    // Configures credentials using the mail account and password
                    Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password),
                    // Enables SSL based on configuration
                    EnableSsl = _mailSettings.EnableSSL,
                };

                // Creates a new mail message instance
                var mailMessage = new MailMessage
                {
                    // Sets the sender address and display name
                    From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName),
                    // Sets the email subject for password reset
                    Subject = "Reset Your Password",
                    // Constructs the HTML body with the reset link
                    Body = $"<p>Hi,</p><p>We received a request to reset your password. Click the link below to reset it:</p>" +
                           $"<p><a href='{resetLink}'>{resetLink}</a></p>" +
                           $"<p>If you didn't request this, you can ignore this email.</p>",
                    // Specifies that the body contains HTML content
                    IsBodyHtml = true,
                };

                // Adds the recipient email address to the message
                mailMessage.To.Add(email);

                // Sends the email asynchronously using the SMTP client
                await smtpClient.SendMailAsync(mailMessage);
                // Logs successful email sending to the console
                Console.WriteLine($"Reset password email sent to {email} successfully.");
            }
            catch (Exception ex)
            {
                // Logs any errors that occur during email sending
                Console.WriteLine($"Failed to send reset password email to {email}. Error: {ex.Message}");
                // Rethrows the exception for higher-level error handling
                throw;
            }
        }

        /// <summary>
        /// Sends an email asynchronously with the specified mail request details.
        /// </summary>
        /// <param name="mailRequest">The mail request containing recipient, subject, body, and attachments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            // Creates a new MIME message instance
            var email = new MimeMessage
            {
                // Sets the sender address from mail settings
                Sender = MailboxAddress.Parse(_mailSettings.Mail)
            };
            // Adds the recipient email address from the mail request
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            // Sets the email subject from the mail request
            email.Subject = mailRequest.Subject;
            // Creates a new body builder for constructing the email body
            var builder = new BodyBuilder();
            // Checks if the mail request contains any attachments
            if (mailRequest.Attachments != null)
            {
                // Declares a variable to hold file bytes
                byte[] fileBytes;
                // Iterates through each attachment in the mail request
                foreach (var file in mailRequest.Attachments)
                {
                    // Checks if the file has content
                    if (file.Length > 0)
                    {
                        // Uses a memory stream to read the file content
                        using (var ms = new MemoryStream())
                        {
                            // Copies the file content to the memory stream
                            file.CopyTo(ms);
                            // Converts the memory stream content to a byte array
                            fileBytes = ms.ToArray();
                        }
                        // Adds the attachment to the email with filename and content type
                        builder.Attachments.Add(file.FileName, fileBytes, MimeKit.ContentType.Parse(file.ContentType));
                    }
                }
            }
            // Sets the HTML body content from the mail request
            builder.HtmlBody = mailRequest.Body;
            // Builds the final email body from the body builder
            email.Body = builder.ToMessageBody();
            // Creates a new SMTP client using MailKit
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            // Connects to the SMTP server with TLS encryption
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            // Authenticates with the SMTP server using credentials
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            // Sends the email asynchronously
            await smtp.SendAsync(email);
            // Disconnects from the SMTP server
            smtp.Disconnect(true);
        }
    }
}
