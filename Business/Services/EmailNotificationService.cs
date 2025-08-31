using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Business.AppSettings;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Phase 7: Email Notification Service
    /// Handles sending email notifications for workflow events and system alerts
    /// </summary>
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly SmtpSettings smtpSettings;
        private readonly ILogger<EmailNotificationService> logger;

        public EmailNotificationService(
            IOptions<SmtpSettings> smtpSettings,
            ILogger<EmailNotificationService> logger)
        {
            this.smtpSettings = smtpSettings.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Send email for workflow events (report submission, approval, rejection)
        /// </summary>
        public async Task<bool> SendWorkflowEmailAsync(WorkflowEmailDto dto)
        {
            try
            {
                var template = GetWorkflowEmailTemplate(dto.WorkflowType, dto.WorkflowStatus);
                var emailBody = BuildEmailBody(template, dto);
                var subject = BuildSubject(template, dto);

                return await SendEmailAsync(dto.RecipientEmail, subject, emailBody, dto.RecipientName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send workflow email to {Email}", dto.RecipientEmail);
                return false;
            }
        }

        /// <summary>
        /// Send system notification email
        /// </summary>
        public async Task<bool> SendSystemNotificationAsync(SystemEmailDto dto)
        {
            try
            {
                var template = GetSystemEmailTemplate(dto.NotificationType);
                var emailBody = BuildSystemEmailBody(template, dto);
                var subject = BuildSystemSubject(template, dto);

                return await SendEmailAsync(dto.RecipientEmail, subject, emailBody, dto.RecipientName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send system email to {Email}", dto.RecipientEmail);
                return false;
            }
        }

        /// <summary>
        /// Send bulk email notifications
        /// </summary>
        public async Task<BulkEmailResultDto> SendBulkEmailAsync(BulkEmailDto dto)
        {
            var result = new BulkEmailResultDto
            {
                TotalEmails = dto.Recipients.Count,
                SuccessfulEmails = 0,
                FailedEmails = 0,
                Results = new List<EmailResultDto>()
            };

            foreach (var recipient in dto.Recipients)
            {
                try
                {
                    var success = await SendEmailAsync(
                        recipient.Email, 
                        dto.Subject, 
                        dto.Body, 
                        recipient.Name
                    );

                    result.Results.Add(new EmailResultDto
                    {
                        Email = recipient.Email,
                        Name = recipient.Name,
                        Success = success,
                        Error = success ? null : "Failed to send email"
                    });

                    if (success)
                    {
                        result.SuccessfulEmails++;
                    }
                    else
                    {
                        result.FailedEmails++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send bulk email to {Email}", recipient.Email);
                    result.Results.Add(new EmailResultDto
                    {
                        Email = recipient.Email,
                        Name = recipient.Name,
                        Success = false,
                        Error = ex.Message
                    });
                    result.FailedEmails++;
                }

                // Add small delay to avoid overwhelming SMTP server
                await Task.Delay(100);
            }

            return result;
        }

        /// <summary>
        /// Send reminder email for due reports
        /// </summary>
        public async Task<bool> SendReminderEmailAsync(ReminderEmailDto dto)
        {
            try
            {
                var template = GetReminderEmailTemplate(dto.ReminderType);
                var emailBody = BuildReminderEmailBody(template, dto);
                var subject = BuildReminderSubject(template, dto);

                return await SendEmailAsync(dto.RecipientEmail, subject, emailBody, dto.RecipientName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send reminder email to {Email}", dto.RecipientEmail);
                return false;
            }
        }

        /// <summary>
        /// Test email configuration
        /// </summary>
        public async Task<bool> TestEmailConfigurationAsync(string testEmail)
        {
            try
            {
                var subject = "Project Controls - Email Configuration Test";
                var body = @"
                    <h2>Email Configuration Test</h2>
                    <p>This is a test email to verify that your email configuration is working correctly.</p>
                    <p><strong>Test Time:</strong> " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
                    <p>If you received this email, your email configuration is working properly.</p>
                    <hr>
                    <p><small>Rand Water Project Controls Reporting Tool</small></p>
                ";

                return await SendEmailAsync(testEmail, subject, body, "Test User");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email configuration test failed for {Email}", testEmail);
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// Core email sending method
        /// </summary>
        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body, string? toName = null)
        {
            if (!smtpSettings.Enabled)
            {
                logger.LogInformation("Email sending is disabled in configuration");
                return false;
            }

            if (string.IsNullOrEmpty(smtpSettings.Host) || smtpSettings.Port <= 0)
            {
                logger.LogError("SMTP configuration is invalid");
                return false;
            }

            try
            {
                using var client = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
                client.EnableSsl = smtpSettings.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings.FromEmail, smtpSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(new MailAddress(toEmail, toName ?? toEmail));

                await client.SendMailAsync(mailMessage);
                logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Get workflow email template
        /// </summary>
        private EmailTemplate GetWorkflowEmailTemplate(WorkflowType workflowType, WorkflowStatus status)
        {
            return workflowType switch
            {
                WorkflowType.ReportSubmission => status switch
                {
                    WorkflowStatus.Submitted => new EmailTemplate
                    {
                        Subject = "Report Submitted: {ReportTitle}",
                        Body = GetReportSubmissionTemplate()
                    },
                    WorkflowStatus.UnderReview => new EmailTemplate
                    {
                        Subject = "Report Under Review: {ReportTitle}",
                        Body = GetReportUnderReviewTemplate()
                    },
                    WorkflowStatus.Approved => new EmailTemplate
                    {
                        Subject = "Report Approved: {ReportTitle}",
                        Body = GetReportApprovedTemplate()
                    },
                    WorkflowStatus.Rejected => new EmailTemplate
                    {
                        Subject = "Report Rejected: {ReportTitle}",
                        Body = GetReportRejectedTemplate()
                    },
                    _ => GetDefaultTemplate()
                },
                _ => GetDefaultTemplate()
            };
        }

        /// <summary>
        /// Get system email template
        /// </summary>
        private EmailTemplate GetSystemEmailTemplate(NotificationType type)
        {
            return type switch
            {
                NotificationType.SystemAlert => new EmailTemplate
                {
                    Subject = "System Alert: {Title}",
                    Body = GetSystemAlertTemplate()
                },
                NotificationType.SystemMaintenance => new EmailTemplate
                {
                    Subject = "System Maintenance: {Title}",
                    Body = GetSystemMaintenanceTemplate()
                },
                NotificationType.SystemUpdate => new EmailTemplate
                {
                    Subject = "System Update: {Title}",
                    Body = GetSystemUpdateTemplate()
                },
                _ => GetDefaultTemplate()
            };
        }

        /// <summary>
        /// Get reminder email template
        /// </summary>
        private EmailTemplate GetReminderEmailTemplate(ReminderType type)
        {
            return type switch
            {
                ReminderType.ReportDue => new EmailTemplate
                {
                    Subject = "Reminder: Report Due Soon - {ReportTitle}",
                    Body = GetReportDueReminderTemplate()
                },
                ReminderType.ReportOverdue => new EmailTemplate
                {
                    Subject = "URGENT: Overdue Report - {ReportTitle}",
                    Body = GetReportOverdueReminderTemplate()
                },
                ReminderType.ReviewPending => new EmailTemplate
                {
                    Subject = "Reminder: Review Pending - {ReportTitle}",
                    Body = GetReviewPendingReminderTemplate()
                },
                _ => GetDefaultTemplate()
            };
        }

        /// <summary>
        /// Build email body from template
        /// </summary>
        private static string BuildEmailBody(EmailTemplate template, WorkflowEmailDto dto)
        {
            return template.Body
                .Replace("{RecipientName}", dto.RecipientName)
                .Replace("{ReportTitle}", dto.ReportTitle)
                .Replace("{ReportId}", dto.ReportId.ToString())
                .Replace("{SubmitterName}", dto.SubmitterName)
                .Replace("{DueDate}", dto.DueDate?.ToString("yyyy-MM-dd") ?? "N/A")
                .Replace("{Comments}", dto.Comments ?? "No comments provided")
                .Replace("{ActionUrl}", dto.ActionUrl ?? "#")
                .Replace("{CurrentDate}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        }

        /// <summary>
        /// Build system email body
        /// </summary>
        private static string BuildSystemEmailBody(EmailTemplate template, SystemEmailDto dto)
        {
            return template.Body
                .Replace("{RecipientName}", dto.RecipientName)
                .Replace("{Title}", dto.Title)
                .Replace("{Message}", dto.Message)
                .Replace("{ActionUrl}", dto.ActionUrl ?? "#")
                .Replace("{CurrentDate}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        }

        /// <summary>
        /// Build reminder email body
        /// </summary>
        private static string BuildReminderEmailBody(EmailTemplate template, ReminderEmailDto dto)
        {
            return template.Body
                .Replace("{RecipientName}", dto.RecipientName)
                .Replace("{ReportTitle}", dto.ReportTitle)
                .Replace("{ReportId}", dto.ReportId.ToString())
                .Replace("{DueDate}", dto.DueDate.ToString("yyyy-MM-dd"))
                .Replace("{DaysOverdue}", dto.DaysOverdue?.ToString() ?? "0")
                .Replace("{ActionUrl}", dto.ActionUrl ?? "#")
                .Replace("{CurrentDate}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        }

        /// <summary>
        /// Build email subject
        /// </summary>
        private static string BuildSubject(EmailTemplate template, WorkflowEmailDto dto)
        {
            return template.Subject
                .Replace("{ReportTitle}", dto.ReportTitle)
                .Replace("{ReportId}", dto.ReportId.ToString());
        }

        /// <summary>
        /// Build system email subject
        /// </summary>
        private static string BuildSystemSubject(EmailTemplate template, SystemEmailDto dto)
        {
            return template.Subject.Replace("{Title}", dto.Title);
        }

        /// <summary>
        /// Build reminder email subject
        /// </summary>
        private static string BuildReminderSubject(EmailTemplate template, ReminderEmailDto dto)
        {
            return template.Subject
                .Replace("{ReportTitle}", dto.ReportTitle)
                .Replace("{ReportId}", dto.ReportId.ToString());
        }

        #endregion

        #region Email Templates

        private static string GetReportSubmissionTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Report Submitted</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #2E86AB 0%, #A23B72 50%, #F18F01 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>Rand Water Project Controls</h1>
            <p style='margin: 5px 0 0 0;'>Report Submission Notification</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #2E86AB;'>Report Submitted Successfully</h2>
            <p>Dear {RecipientName},</p>
            <p>A new report has been submitted and is now available for review.</p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #2E86AB; margin: 20px 0;'>
                <strong>Report Details:</strong><br>
                <strong>Title:</strong> {ReportTitle}<br>
                <strong>Report ID:</strong> {ReportId}<br>
                <strong>Submitted By:</strong> {SubmitterName}<br>
                <strong>Due Date:</strong> {DueDate}<br>
                <strong>Submitted On:</strong> {CurrentDate}
            </div>
            
            <p><strong>Next Steps:</strong></p>
            <ul>
                <li>The report is now under review</li>
                <li>You will be notified when review is complete</li>
                <li>Please ensure all supporting documents are attached</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #2E86AB; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Report</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
            <p>Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetReportApprovedTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Report Approved</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #4CAF50 0%, #2E86AB 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>✓ Report Approved</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #4CAF50;'>Congratulations! Your Report Has Been Approved</h2>
            <p>Dear {RecipientName},</p>
            <p>Your report has been reviewed and approved.</p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0;'>
                <strong>Report Details:</strong><br>
                <strong>Title:</strong> {ReportTitle}<br>
                <strong>Report ID:</strong> {ReportId}<br>
                <strong>Approved On:</strong> {CurrentDate}<br>
                <strong>Comments:</strong> {Comments}
            </div>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Approved Report</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetReportRejectedTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Report Rejected</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #f44336 0%, #A23B72 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>⚠ Report Requires Revision</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #f44336;'>Report Needs Revision</h2>
            <p>Dear {RecipientName},</p>
            <p>Your report has been reviewed and requires some revisions before it can be approved.</p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0;'>
                <strong>Report Details:</strong><br>
                <strong>Title:</strong> {ReportTitle}<br>
                <strong>Report ID:</strong> {ReportId}<br>
                <strong>Reviewed On:</strong> {CurrentDate}<br>
                <strong>Feedback:</strong> {Comments}
            </div>
            
            <p><strong>Next Steps:</strong></p>
            <ul>
                <li>Review the feedback provided above</li>
                <li>Make the necessary revisions to your report</li>
                <li>Resubmit the report for review</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #f44336; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Revise Report</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetReportUnderReviewTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Report Under Review</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #FF9800 0%, #2E86AB 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>📋 Report Under Review</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #FF9800;'>Report Review in Progress</h2>
            <p>Dear {RecipientName},</p>
            <p>A report has been assigned to you for review.</p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #FF9800; margin: 20px 0;'>
                <strong>Report Details:</strong><br>
                <strong>Title:</strong> {ReportTitle}<br>
                <strong>Report ID:</strong> {ReportId}<br>
                <strong>Submitted By:</strong> {SubmitterName}<br>
                <strong>Due Date:</strong> {DueDate}<br>
                <strong>Review Started:</strong> {CurrentDate}
            </div>
            
            <p><strong>Please Review:</strong></p>
            <ul>
                <li>All report content for accuracy and completeness</li>
                <li>Supporting documentation and attachments</li>
                <li>Compliance with reporting standards</li>
                <li>Provide feedback if revisions are needed</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #FF9800; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Review Report</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetReportDueReminderTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Report Due Reminder</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #FF9800 0%, #F18F01 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>⏰ Report Due Reminder</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #FF9800;'>Report Due Soon</h2>
            <p>Dear {RecipientName},</p>
            <p>This is a friendly reminder that your report is due soon.</p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #FF9800; margin: 20px 0;'>
                <strong>Report Details:</strong><br>
                <strong>Title:</strong> {ReportTitle}<br>
                <strong>Report ID:</strong> {ReportId}<br>
                <strong>Due Date:</strong> {DueDate}<br>
                <strong>Reminder Sent:</strong> {CurrentDate}
            </div>
            
            <p><strong>Action Required:</strong></p>
            <ul>
                <li>Complete your report before the due date</li>
                <li>Ensure all required sections are filled</li>
                <li>Attach any supporting documents</li>
                <li>Submit for review</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #FF9800; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Complete Report</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetReportOverdueReminderTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>URGENT: Overdue Report</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #f44336 0%, #d32f2f 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>🚨 URGENT: Overdue Report</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #f44336;'>URGENT: Report Overdue</h2>
            <p>Dear {RecipientName},</p>
            <p><strong>Your report is now overdue and requires immediate attention.</strong></p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0;'>
                <strong>Report Details:</strong><br>
                <strong>Title:</strong> {ReportTitle}<br>
                <strong>Report ID:</strong> {ReportId}<br>
                <strong>Due Date:</strong> {DueDate}<br>
                <strong>Days Overdue:</strong> {DaysOverdue} days<br>
                <strong>Notice Sent:</strong> {CurrentDate}
            </div>
            
            <p><strong>IMMEDIATE ACTION REQUIRED:</strong></p>
            <ul style='color: #f44336;'>
                <li><strong>Complete and submit your report immediately</strong></li>
                <li><strong>Contact your line manager if assistance is needed</strong></li>
                <li><strong>Explain any delays in the comments section</strong></li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #f44336; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>SUBMIT REPORT NOW</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetReviewPendingReminderTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Review Pending Reminder</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #673AB7 0%, #2E86AB 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>📝 Review Pending</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #673AB7;'>Report Review Pending</h2>
            <p>Dear {RecipientName},</p>
            <p>You have a report pending review that requires your attention.</p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #673AB7; margin: 20px 0;'>
                <strong>Report Details:</strong><br>
                <strong>Title:</strong> {ReportTitle}<br>
                <strong>Report ID:</strong> {ReportId}<br>
                <strong>Due Date:</strong> {DueDate}<br>
                <strong>Reminder Sent:</strong> {CurrentDate}
            </div>
            
            <p><strong>Please Review and:</strong></p>
            <ul>
                <li>Approve if the report meets all requirements</li>
                <li>Reject with feedback if revisions are needed</li>
                <li>Contact the submitter if clarification is required</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #673AB7; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Review Report</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetSystemAlertTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>System Alert</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #f44336 0%, #A23B72 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>🔔 System Alert</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #f44336;'>{Title}</h2>
            <p>Dear {RecipientName},</p>
            <p>{Message}</p>
            
            <div style='background: white; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0;'>
                <strong>Alert Details:</strong><br>
                <strong>Alert Time:</strong> {CurrentDate}<br>
                <strong>Priority:</strong> High
            </div>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #f44336; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Details</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated system alert from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetSystemMaintenanceTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>System Maintenance</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #FF9800 0%, #F18F01 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>🔧 System Maintenance</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #FF9800;'>{Title}</h2>
            <p>Dear {RecipientName},</p>
            <p>{Message}</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #FF9800; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>More Information</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated maintenance notification from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetSystemUpdateTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>System Update</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #4CAF50 0%, #2E86AB 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>🚀 System Update</h1>
            <p style='margin: 5px 0 0 0;'>Rand Water Project Controls</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <h2 style='color: #4CAF50;'>{Title}</h2>
            <p>Dear {RecipientName},</p>
            <p>{Message}</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Learn More</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated update notification from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static EmailTemplate GetDefaultTemplate()
        {
            return new EmailTemplate
            {
                Subject = "Notification from Rand Water Project Controls",
                Body = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Notification</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background: linear-gradient(135deg, #2E86AB 0%, #A23B72 50%, #F18F01 100%); color: white; padding: 20px; text-align: center;'>
            <h1 style='margin: 0;'>Rand Water Project Controls</h1>
            <p style='margin: 5px 0 0 0;'>Notification</p>
        </div>
        
        <div style='padding: 20px; background: #f9f9f9;'>
            <p>Dear {RecipientName},</p>
            <p>You have received a notification from the Project Controls Reporting Tool.</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{ActionUrl}' style='background: #2E86AB; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Details</a>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; color: #666; font-size: 12px;'>
            <p>This is an automated message from the Rand Water Project Controls Reporting Tool.</p>
        </div>
    </div>
</body>
</html>"
            };
        }

        #endregion
    }

    /// <summary>
    /// Email template structure
    /// </summary>
    public class EmailTemplate
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
