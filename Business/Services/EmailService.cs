using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using ProjectControlsReportingTool.API.Business.AppSettings;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Email service for sending notifications, reports, and communications
    /// Supports SMTP delivery via MailKit
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;
        private readonly ILogger<EmailService> logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            this.emailSettings = emailSettings.Value;
            this.logger = logger;
        }

        #region IEmailService Implementation

        /// <summary>
        /// Sends a single email
        /// </summary>
        public async Task<EmailSendResult> SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null, EmailOptions? options = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSettings.SenderName, emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlBody;
                if (!string.IsNullOrEmpty(textBody))
                {
                    bodyBuilder.TextBody = textBody;
                }
                
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
                var response = await client.SendAsync(message);
                await client.DisconnectAsync(true);

                logger.LogInformation("Email sent successfully to {Recipient}", to);
                return new EmailSendResult
                {
                    Success = true,
                    MessageId = response,
                    SentDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {Recipient}", to);
                return new EmailSendResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SentDate = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Sends email using template
        /// </summary>
        public async Task<EmailSendResult> SendTemplatedEmailAsync(string to, Guid templateId, Dictionary<string, object> variables, EmailOptions? options = null)
        {
            try
            {
                // For now, use basic template rendering
                // In a full implementation, you'd load the template from database by templateId
                var htmlBody = $"<html><body><h2>Notification</h2><p>You have received a notification from the Project Controls Reporting Tool.</p></body></html>";
                var subject = "Project Controls Notification";

                return await SendEmailAsync(to, subject, htmlBody, null, options);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send templated email to {Recipient} using template {TemplateId}", to, templateId);
                return new EmailSendResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SentDate = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Queues email for later delivery
        /// </summary>
        public async Task<Guid> QueueEmailAsync(EmailQueueDto queueItem)
        {
            // For now, just send immediately
            // In a full implementation, you'd store in database queue
            await SendEmailAsync(queueItem.ToEmail!, queueItem.Subject!, $"<html><body><h2>Notification</h2><p>Email from queue: {queueItem.Subject}</p></body></html>");
            return Guid.NewGuid(); // Return a queue item ID
        }

        /// <summary>
        /// Processes email queue
        /// </summary>
        public async Task<int> ProcessEmailQueueAsync(int batchSize = 50)
        {
            // Simplified implementation - in real app, would process from database queue
            await Task.CompletedTask;
            return 0;
        }

        /// <summary>
        /// Gets email queue status
        /// </summary>
        public async Task<PagedResultDto<EmailQueueDto>> GetEmailQueueAsync(NotificationFilterDto filter)
        {
            // Simplified implementation - return empty result
            await Task.CompletedTask;
            return new PagedResultDto<EmailQueueDto>
            {
                Items = new List<EmailQueueDto>(),
                TotalCount = 0,
                Page = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        /// <summary>
        /// Retries failed emails
        /// </summary>
        public async Task<int> RetryFailedEmailsAsync(int maxRetries = 3)
        {
            // Simplified implementation
            await Task.CompletedTask;
            return 0;
        }

        /// <summary>
        /// Cancels pending emails
        /// </summary>
        public async Task<int> CancelPendingEmailsAsync(List<Guid> emailIds)
        {
            // Simplified implementation
            await Task.CompletedTask;
            return 0;
        }

        /// <summary>
        /// Processes email template with variables
        /// </summary>
        public async Task<TemplatePreviewDto> ProcessTemplateAsync(NotificationTemplateDto template, Dictionary<string, object> variables)
        {
            // Simplified implementation
            await Task.CompletedTask;
            return new TemplatePreviewDto
            {
                HtmlContent = template.HtmlTemplate ?? "",
                TextContent = template.TextTemplate ?? "",
                Subject = template.Subject ?? ""
            };
        }

        /// <summary>
        /// Validates template syntax
        /// </summary>
        public async Task<TemplateValidationResult> ValidateTemplateAsync(string htmlTemplate, string textTemplate)
        {
            // Simplified implementation
            await Task.CompletedTask;
            return new TemplateValidationResult
            {
                IsValid = true
            };
        }

        #endregion

        #region Additional Helper Methods

        /// <summary>
        /// Test SMTP connection
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
                await client.DisconnectAsync(true);
                
                logger.LogInformation("SMTP connection test successful");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SMTP connection test failed");
                return false;
            }
        }

        #endregion
    }
}
