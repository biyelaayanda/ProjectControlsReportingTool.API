using AutoMapper;
using Microsoft.Extensions.Logging;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Repositories;
using RazorLight;

namespace ProjectControlsReportingTool.API.Business.Services
{
    /// <summary>
    /// Service implementation for email template management operations
    /// Provides business logic for creating, managing, and rendering email templates
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailTemplateRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly IRazorLightEngine _razorEngine;

        public EmailTemplateService(
            IEmailTemplateRepository repository,
            IMapper mapper,
            ILogger<EmailTemplateService> logger,
            IRazorLightEngine razorEngine)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _razorEngine = razorEngine;
        }

        #region CRUD Operations

        public async Task<List<EmailTemplateDto>> GetAllAsync(EmailTemplateSearchDto? searchDto = null)
        {
            try
            {
                var templates = await _repository.GetAllAsync(searchDto);
                return _mapper.Map<List<EmailTemplateDto>>(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email templates");
                throw;
            }
        }

        public async Task<EmailTemplateDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var template = await _repository.GetByIdAsync(id);
                return template != null ? _mapper.Map<EmailTemplateDto>(template) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email template with ID: {TemplateId}", id);
                throw;
            }
        }

        public async Task<EmailTemplateDto?> GetByNameAsync(string name)
        {
            try
            {
                var template = await _repository.GetByNameAsync(name);
                return template != null ? _mapper.Map<EmailTemplateDto>(template) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email template with name: {TemplateName}", name);
                throw;
            }
        }

        public async Task<EmailTemplateDto> CreateAsync(CreateEmailTemplateDto createDto, Guid userId)
        {
            try
            {
                // Validate template name uniqueness
                if (await _repository.ExistsByNameAsync(createDto.Name))
                {
                    throw new InvalidOperationException($"Template with name '{createDto.Name}' already exists.");
                }

                // Validate template syntax
                var validationResult = await ValidateTemplateSyntaxAsync(createDto.HtmlContent, createDto.Subject);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException($"Template validation failed: {string.Join(", ", validationResult.ValidationErrors)}");
                }

                var template = _mapper.Map<EmailTemplate>(createDto);
                template.CreatedBy = userId;
                template.UpdatedBy = userId;
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;

                // Set version and extract variables
                template.Version = 1;
                template.Variables = ExtractTemplateVariables(createDto.HtmlContent, createDto.Subject);

                var createdTemplate = await _repository.CreateAsync(template);
                
                _logger.LogInformation("Email template created successfully. ID: {TemplateId}, Name: {TemplateName}", 
                    createdTemplate.Id, createdTemplate.Name);

                return _mapper.Map<EmailTemplateDto>(createdTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email template: {TemplateName}", createDto.Name);
                throw;
            }
        }

        public async Task<EmailTemplateDto?> UpdateAsync(Guid id, UpdateEmailTemplateDto updateDto, Guid userId)
        {
            try
            {
                var existingTemplate = await _repository.GetByIdAsync(id);
                if (existingTemplate == null)
                {
                    return null;
                }

                // Validate system template restrictions
                if (existingTemplate.IsSystemTemplate && 
                    (updateDto.Name != null || updateDto.Category != null))
                {
                    throw new InvalidOperationException("Cannot modify core properties of system templates.");
                }

                // Validate name uniqueness if name is being changed
                if (!string.IsNullOrWhiteSpace(updateDto.Name) && 
                    updateDto.Name != existingTemplate.Name &&
                    await _repository.ExistsByNameAsync(updateDto.Name, id))
                {
                    throw new InvalidOperationException($"Template with name '{updateDto.Name}' already exists.");
                }

                // Apply updates
                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                    existingTemplate.Name = updateDto.Name;
                
                if (!string.IsNullOrWhiteSpace(updateDto.Description))
                    existingTemplate.Description = updateDto.Description;
                
                if (!string.IsNullOrWhiteSpace(updateDto.Subject))
                    existingTemplate.Subject = updateDto.Subject;
                
                if (!string.IsNullOrWhiteSpace(updateDto.HtmlContent))
                    existingTemplate.HtmlContent = updateDto.HtmlContent;
                
                if (!string.IsNullOrWhiteSpace(updateDto.PlainTextContent))
                    existingTemplate.PlainTextContent = updateDto.PlainTextContent;
                
                if (!string.IsNullOrWhiteSpace(updateDto.Category))
                    existingTemplate.Category = updateDto.Category;
                
                if (updateDto.IsActive.HasValue)
                    existingTemplate.IsActive = updateDto.IsActive.Value;

                // Validate updated template syntax
                var validationResult = await ValidateTemplateSyntaxAsync(existingTemplate.HtmlContent, existingTemplate.Subject);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException($"Template validation failed: {string.Join(", ", validationResult.ValidationErrors)}");
                }

                // Update metadata
                existingTemplate.UpdatedBy = userId;
                existingTemplate.Variables = ExtractTemplateVariables(existingTemplate.HtmlContent, existingTemplate.Subject);

                var updatedTemplate = await _repository.UpdateAsync(existingTemplate);
                
                _logger.LogInformation("Email template updated successfully. ID: {TemplateId}, Name: {TemplateName}", 
                    updatedTemplate.Id, updatedTemplate.Name);

                return _mapper.Map<EmailTemplateDto>(updatedTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email template with ID: {TemplateId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            try
            {
                var template = await _repository.GetByIdAsync(id);
                if (template == null)
                {
                    return false;
                }

                if (template.IsSystemTemplate)
                {
                    throw new InvalidOperationException("Cannot delete system templates.");
                }

                if (!await _repository.CanDeleteAsync(id))
                {
                    throw new InvalidOperationException("Template cannot be deleted because it is in use.");
                }

                var result = await _repository.DeleteAsync(id);
                
                if (result)
                {
                    _logger.LogInformation("Email template deleted successfully. ID: {TemplateId}, Name: {TemplateName}", 
                        id, template.Name);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting email template with ID: {TemplateId}", id);
                throw;
            }
        }

        public async Task<EmailTemplateDto?> DuplicateAsync(Guid id, DuplicateEmailTemplateDto duplicateDto, Guid userId)
        {
            try
            {
                var sourceTemplate = await _repository.GetByIdAsync(id);
                if (sourceTemplate == null)
                {
                    return null;
                }

                var createDto = new CreateEmailTemplateDto
                {
                    Name = duplicateDto.NewName,
                    Description = duplicateDto.NewDescription ?? $"Copy of {sourceTemplate.Description}",
                    TemplateType = sourceTemplate.TemplateType,
                    Subject = sourceTemplate.Subject,
                    HtmlContent = sourceTemplate.HtmlContent,
                    PlainTextContent = sourceTemplate.PlainTextContent,
                    Category = duplicateDto.NewCategory ?? sourceTemplate.Category,
                    IsActive = duplicateDto.SetAsActive
                };

                return await CreateAsync(createDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating email template with ID: {TemplateId}", id);
                throw;
            }
        }

        #endregion

        #region Template Rendering and Preview

        public async Task<RenderedEmailTemplateDto?> RenderTemplateAsync(Guid templateId, Dictionary<string, object> data)
        {
            try
            {
                var template = await _repository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return null;
                }

                await _repository.IncrementUsageAsync(templateId);

                return await RenderTemplateInternal(template, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering email template with ID: {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<RenderedEmailTemplateDto?> RenderTemplateByNameAsync(string templateName, Dictionary<string, object> data)
        {
            try
            {
                var template = await _repository.GetByNameAsync(templateName);
                if (template == null)
                {
                    return null;
                }

                await _repository.IncrementUsageAsync(template.Id);

                return await RenderTemplateInternal(template, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering email template with name: {TemplateName}", templateName);
                throw;
            }
        }

        public async Task<RenderedEmailTemplateDto> PreviewTemplateAsync(EmailTemplatePreviewDto previewDto)
        {
            try
            {
                var template = await _repository.GetByIdAsync(previewDto.TemplateId);
                if (template == null)
                {
                    throw new InvalidOperationException($"Template with ID {previewDto.TemplateId} not found.");
                }

                return await RenderTemplateInternal(template, previewDto.TestData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing email template with ID: {TemplateId}", previewDto.TemplateId);
                throw;
            }
        }

        public async Task<EmailTemplateValidationDto> ValidateTemplateAsync(string htmlContent, string subject)
        {
            try
            {
                var result = await ValidateTemplateSyntaxAsync(htmlContent, subject);
                
                return new EmailTemplateValidationDto
                {
                    IsValid = result.IsValid,
                    Errors = result.ValidationErrors,
                    Warnings = new List<string>(),
                    DetectedVariables = result.DetectedVariables,
                    SuggestedCorrections = new Dictionary<string, string>(),
                    EstimatedRenderTime = 100 // Default estimate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email template");
                return new EmailTemplateValidationDto
                {
                    IsValid = false,
                    Errors = new List<string> { $"Validation error: {ex.Message}" },
                    Warnings = new List<string>(),
                    DetectedVariables = new List<string>(),
                    SuggestedCorrections = new Dictionary<string, string>(),
                    EstimatedRenderTime = 0
                };
            }
        }

        #endregion

        #region Template Management

        public async Task<List<EmailTemplateDto>> GetByTypeAsync(string templateType, bool includeInactive = false)
        {
            try
            {
                var templates = await _repository.GetByTypeAsync(templateType, includeInactive);
                return _mapper.Map<List<EmailTemplateDto>>(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email templates by type: {TemplateType}", templateType);
                throw;
            }
        }

        public async Task<List<EmailTemplateDto>> GetByCategoryAsync(string category, bool includeInactive = false)
        {
            try
            {
                var templates = await _repository.GetByCategoryAsync(category, includeInactive);
                return _mapper.Map<List<EmailTemplateDto>>(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email templates by category: {Category}", category);
                throw;
            }
        }

        public async Task<bool> SetAsDefaultAsync(Guid id, Guid userId)
        {
            try
            {
                var template = await _repository.GetByIdAsync(id);
                if (template == null)
                {
                    return false;
                }

                var result = await _repository.SetAsDefaultAsync(id, template.TemplateType);
                
                if (result)
                {
                    _logger.LogInformation("Email template set as default. ID: {TemplateId}, Type: {TemplateType}", 
                        id, template.TemplateType);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting email template as default. ID: {TemplateId}", id);
                throw;
            }
        }

        public async Task<EmailTemplateDto?> ToggleActiveStatusAsync(Guid id, Guid userId)
        {
            try
            {
                var template = await _repository.GetByIdAsync(id);
                if (template == null)
                {
                    return null;
                }

                template.IsActive = !template.IsActive;
                template.UpdatedBy = userId;

                var updatedTemplate = await _repository.UpdateAsync(template);
                
                _logger.LogInformation("Email template active status toggled. ID: {TemplateId}, IsActive: {IsActive}", 
                    id, updatedTemplate.IsActive);

                return _mapper.Map<EmailTemplateDto>(updatedTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling email template active status. ID: {TemplateId}", id);
                throw;
            }
        }

        public async Task<EmailTemplateStatsDto?> GetTemplateStatsAsync(Guid id)
        {
            try
            {
                var template = await _repository.GetByIdAsync(id);
                if (template == null)
                {
                    return null;
                }

                var (usageCount, lastUsed) = await _repository.GetUsageStatsAsync(id);

                return new EmailTemplateStatsDto
                {
                    TotalTemplates = 1,
                    ActiveTemplates = template.IsActive ? 1 : 0,
                    InactiveTemplates = template.IsActive ? 0 : 1,
                    SystemTemplates = template.IsSystemTemplate ? 1 : 0,
                    UserTemplates = template.IsSystemTemplate ? 0 : 1,
                    DefaultTemplates = template.IsDefault ? 1 : 0,
                    TemplatesByType = new Dictionary<string, int> { { template.TemplateType, 1 } },
                    TemplatesByCategory = new Dictionary<string, int> { { template.Category, 1 } },
                    TemplatesByCreator = new Dictionary<string, int> { { "Unknown", 1 } },
                    TotalUsageCount = usageCount,
                    MostUsedTemplate = new EmailTemplateUsageStatsDto
                    {
                        Id = id,
                        Name = template.Name,
                        TemplateType = template.TemplateType,
                        Category = template.Category,
                        UsageCount = usageCount,
                        LastUsed = lastUsed,
                        CreatedAt = template.CreatedAt,
                        CreatedByName = "Unknown"
                    },
                    RecentlyCreated = new EmailTemplateUsageStatsDto
                    {
                        Id = id,
                        Name = template.Name,
                        TemplateType = template.TemplateType,
                        Category = template.Category,
                        UsageCount = usageCount,
                        LastUsed = lastUsed,
                        CreatedAt = template.CreatedAt,
                        CreatedByName = "Unknown"
                    },
                    RecentlyUpdated = new EmailTemplateUsageStatsDto
                    {
                        Id = id,
                        Name = template.Name,
                        TemplateType = template.TemplateType,
                        Category = template.Category,
                        UsageCount = usageCount,
                        LastUsed = lastUsed,
                        CreatedAt = template.CreatedAt,
                        CreatedByName = "Unknown"
                    },
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email template statistics. ID: {TemplateId}", id);
                throw;
            }
        }

        public async Task RecordTemplateUsageAsync(Guid templateId)
        {
            try
            {
                await _repository.IncrementUsageAsync(templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording email template usage. ID: {TemplateId}", templateId);
                // Don't throw here as this is typically called in background operations
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<int> BulkOperationAsync(BulkEmailTemplateOperationDto operationDto, Guid userId)
        {
            try
            {
                return operationDto.Operation.ToLowerInvariant() switch
                {
                    "activate" => await _repository.BulkUpdateActiveStatusAsync(operationDto.TemplateIds, true),
                    "deactivate" => await _repository.BulkUpdateActiveStatusAsync(operationDto.TemplateIds, false),
                    "delete" => await _repository.BulkDeleteAsync(operationDto.TemplateIds),
                    "updatecategory" => await _repository.BulkUpdateCategoryAsync(operationDto.TemplateIds, operationDto.Category ?? "General"),
                    _ => throw new InvalidOperationException($"Unsupported bulk operation: {operationDto.Operation}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operation: {Operation} on {Count} templates", 
                    operationDto.Operation, operationDto.TemplateIds.Count);
                throw;
            }
        }

        public async Task<EmailTemplateExportDto> ExportTemplatesAsync(List<Guid>? templateIds = null)
        {
            try
            {
                List<EmailTemplate> templatesToExport;
                
                if (templateIds != null && templateIds.Any())
                {
                    templatesToExport = await _repository.GetByIdsAsync(templateIds);
                }
                else
                {
                    templatesToExport = await _repository.GetAllAsync();
                }

                var exportDto = new EmailTemplateExportDto
                {
                    TemplateIds = templatesToExport.Select(t => t.Id).ToList(),
                    Format = "json",
                    IncludeSystemTemplates = templatesToExport.Any(t => t.IsSystemTemplate),
                    IncludeUsageStats = true
                };

                _logger.LogInformation("Exported {Count} email templates", templatesToExport.Count);

                return exportDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting email templates");
                throw;
            }
        }

        public Task<EmailTemplateImportDto> ImportTemplatesAsync(EmailTemplateImportDto importDto, Guid userId)
        {
            try
            {
                // For now, return a simple success result
                // In a real implementation, you would parse the Content property and process templates
                _logger.LogInformation("Template import requested by user {UserId} with format {Format}", userId, importDto.Format);
                
                // Return the same DTO to indicate it was processed
                return Task.FromResult(importDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing email templates");
                throw;
            }
        }

        #endregion

        #region System Template Management

        public async Task<int> CreateDefaultSystemTemplatesAsync(Guid userId)
        {
            try
            {
                var defaultTemplates = GetDefaultSystemTemplates(userId);
                var createdCount = 0;

                foreach (var template in defaultTemplates)
                {
                    try
                    {
                        // Check if system template already exists
                        var existing = await _repository.GetByNameAsync(template.Name);
                        if (existing == null)
                        {
                            await _repository.CreateAsync(template);
                            createdCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create system template: {TemplateName}", template.Name);
                    }
                }

                _logger.LogInformation("Created {Count} default system templates", createdCount);
                return createdCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default system templates");
                throw;
            }
        }

        public async Task<List<EmailTemplateDto>> GetSystemTemplatesAsync()
        {
            try
            {
                var templates = await _repository.GetSystemTemplatesAsync();
                return _mapper.Map<List<EmailTemplateDto>>(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system templates");
                throw;
            }
        }

        public async Task<bool> ResetSystemTemplateAsync(string templateName, Guid userId)
        {
            try
            {
                var existingTemplate = await _repository.GetByNameAsync(templateName);
                if (existingTemplate == null || !existingTemplate.IsSystemTemplate)
                {
                    return false;
                }

                var defaultTemplate = GetDefaultSystemTemplates(userId)
                    .FirstOrDefault(t => t.Name == templateName);
                
                if (defaultTemplate == null)
                {
                    return false;
                }

                // Reset to default values
                existingTemplate.Subject = defaultTemplate.Subject;
                existingTemplate.HtmlContent = defaultTemplate.HtmlContent;
                existingTemplate.PlainTextContent = defaultTemplate.PlainTextContent;
                existingTemplate.Variables = defaultTemplate.Variables;
                existingTemplate.UpdatedBy = userId;

                await _repository.UpdateAsync(existingTemplate);
                
                _logger.LogInformation("System template reset to default: {TemplateName}", templateName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting system template: {TemplateName}", templateName);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<RenderedEmailTemplateDto> RenderTemplateInternal(EmailTemplate template, Dictionary<string, object> data)
        {
            var result = new RenderedEmailTemplateDto
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                UsedVariables = data,
                RenderedAt = DateTime.UtcNow
            };

            try
            {
                // Get template variables
                var templateVariables = template.GetVariableNames();
                result.MissingVariables = templateVariables.Where(v => !data.ContainsKey(v)).ToList();

                // Render subject
                result.Subject = await _razorEngine.CompileRenderStringAsync(
                    $"subject_{template.Id}", template.Subject, data);

                // Render HTML content
                result.HtmlContent = await _razorEngine.CompileRenderStringAsync(
                    $"html_{template.Id}", template.HtmlContent, data);

                // Render plain text content if available
                if (!string.IsNullOrWhiteSpace(template.PlainTextContent))
                {
                    result.PlainTextContent = await _razorEngine.CompileRenderStringAsync(
                        $"text_{template.Id}", template.PlainTextContent, data);
                }

                result.IsValid = true;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Rendering error: {ex.Message}");
            }

            return result;
        }

        private async Task<(bool IsValid, List<string> ValidationErrors, List<string> DetectedVariables)> ValidateTemplateSyntaxAsync(string htmlContent, string subject)
        {
            var errors = new List<string>();
            var variables = new HashSet<string>();

            try
            {
                // Extract variables from content
                var htmlVariables = ExtractVariablesFromText(htmlContent);
                var subjectVariables = ExtractVariablesFromText(subject);
                
                foreach (var variable in htmlVariables.Concat(subjectVariables))
                {
                    variables.Add(variable);
                }

                // Test compile the templates with empty data
                var testData = new Dictionary<string, object>();
                
                await _razorEngine.CompileRenderStringAsync("test_subject", subject, testData);
                await _razorEngine.CompileRenderStringAsync("test_html", htmlContent, testData);
            }
            catch (Exception ex)
            {
                errors.Add($"Template syntax error: {ex.Message}");
            }

            return (errors.Count == 0, errors, variables.ToList());
        }

        private static List<string> ExtractVariablesFromText(string text)
        {
            var variables = new HashSet<string>();
            
            // Match @Model.VariableName patterns
            var modelMatches = System.Text.RegularExpressions.Regex.Matches(text, @"@Model\.([a-zA-Z_][a-zA-Z0-9_]*)");
            foreach (System.Text.RegularExpressions.Match match in modelMatches)
            {
                variables.Add(match.Groups[1].Value);
            }

            // Match @(Model.VariableName) patterns
            var parenMatches = System.Text.RegularExpressions.Regex.Matches(text, @"@\(Model\.([a-zA-Z_][a-zA-Z0-9_]*)\)");
            foreach (System.Text.RegularExpressions.Match match in parenMatches)
            {
                variables.Add(match.Groups[1].Value);
            }

            return variables.ToList();
        }

        private string ExtractTemplateVariables(string htmlContent, string subject)
        {
            var variables = new HashSet<string>();
            
            // Simple regex to extract @Model.VariableName patterns
            var regex = new System.Text.RegularExpressions.Regex(@"@Model\.(\w+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var subjectMatches = regex.Matches(subject);
            var htmlMatches = regex.Matches(htmlContent);

            foreach (System.Text.RegularExpressions.Match match in subjectMatches)
            {
                variables.Add(match.Groups[1].Value);
            }

            foreach (System.Text.RegularExpressions.Match match in htmlMatches)
            {
                variables.Add(match.Groups[1].Value);
            }

            return System.Text.Json.JsonSerializer.Serialize(variables.ToList());
        }

        private List<EmailTemplate> GetDefaultSystemTemplates(Guid userId)
        {
            return new List<EmailTemplate>
            {
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Welcome Email",
                    Description = "Welcome email for new users",
                    TemplateType = "Welcome",
                    Subject = "Welcome to Project Controls Reporting Tool - @Model.UserName",
                    HtmlContent = @"
                        <h2>Welcome @Model.UserName!</h2>
                        <p>Your account has been created successfully.</p>
                        <p>Login URL: <a href='@Model.LoginUrl'>@Model.LoginUrl</a></p>
                        <p>If you have any questions, please contact support.</p>",
                    PlainTextContent = @"
                        Welcome @Model.UserName!
                        Your account has been created successfully.
                        Login URL: @Model.LoginUrl
                        If you have any questions, please contact support.",
                    Category = "System",
                    IsActive = true,
                    IsSystemTemplate = true,
                    IsDefault = true,
                    CreatedBy = userId,
                    UpdatedBy = userId
                },
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Report Status Notification",
                    Description = "Notification when report status changes",
                    TemplateType = "ReportStatus",
                    Subject = "Report Status Update - @Model.ReportTitle",
                    HtmlContent = @"
                        <h2>Report Status Update</h2>
                        <p>The status of report '@Model.ReportTitle' has been updated.</p>
                        <p><strong>New Status:</strong> @Model.NewStatus</p>
                        <p><strong>Updated By:</strong> @Model.UpdatedBy</p>
                        <p><strong>Comments:</strong> @Model.Comments</p>
                        <p><a href='@Model.ReportUrl'>View Report</a></p>",
                    PlainTextContent = @"
                        Report Status Update
                        The status of report '@Model.ReportTitle' has been updated.
                        New Status: @Model.NewStatus
                        Updated By: @Model.UpdatedBy
                        Comments: @Model.Comments
                        View Report: @Model.ReportUrl",
                    Category = "System",
                    IsActive = true,
                    IsSystemTemplate = true,
                    IsDefault = true,
                    CreatedBy = userId,
                    UpdatedBy = userId
                },
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Password Reset",
                    Description = "Password reset email template",
                    TemplateType = "PasswordReset",
                    Subject = "Password Reset Request",
                    HtmlContent = @"
                        <h2>Password Reset Request</h2>
                        <p>You have requested to reset your password.</p>
                        <p>Click the link below to reset your password:</p>
                        <p><a href='@Model.ResetUrl'>Reset Password</a></p>
                        <p>This link will expire in @Model.ExpiryMinutes minutes.</p>
                        <p>If you did not request this, please ignore this email.</p>",
                    PlainTextContent = @"
                        Password Reset Request
                        You have requested to reset your password.
                        Reset Password: @Model.ResetUrl
                        This link will expire in @Model.ExpiryMinutes minutes.
                        If you did not request this, please ignore this email.",
                    Category = "System",
                    IsActive = true,
                    IsSystemTemplate = true,
                    IsDefault = true,
                    CreatedBy = userId,
                    UpdatedBy = userId
                }
            };
        }

        #endregion
    }
}
