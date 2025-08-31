using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    /// <summary>
    /// Service interface for email template management operations
    /// Provides business logic for creating, managing, and rendering email templates
    /// </summary>
    public interface IEmailTemplateService
    {
        #region CRUD Operations

        /// <summary>
        /// Get all email templates with optional filtering
        /// </summary>
        /// <param name="searchDto">Optional search criteria</param>
        /// <returns>List of email template DTOs</returns>
        Task<List<EmailTemplateDto>> GetAllAsync(EmailTemplateSearchDto? searchDto = null);

        /// <summary>
        /// Get email template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Email template DTO or null if not found</returns>
        Task<EmailTemplateDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get email template by name
        /// </summary>
        /// <param name="name">Template name</param>
        /// <returns>Email template DTO or null if not found</returns>
        Task<EmailTemplateDto?> GetByNameAsync(string name);

        /// <summary>
        /// Create a new email template
        /// </summary>
        /// <param name="createDto">Template creation data</param>
        /// <param name="userId">ID of the user creating the template</param>
        /// <returns>Created email template DTO</returns>
        Task<EmailTemplateDto> CreateAsync(CreateEmailTemplateDto createDto, Guid userId);

        /// <summary>
        /// Update an existing email template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="updateDto">Template update data</param>
        /// <param name="userId">ID of the user updating the template</param>
        /// <returns>Updated email template DTO or null if not found</returns>
        Task<EmailTemplateDto?> UpdateAsync(Guid id, UpdateEmailTemplateDto updateDto, Guid userId);

        /// <summary>
        /// Delete an email template (only non-system templates)
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="userId">ID of the user deleting the template</param>
        /// <returns>True if deleted successfully, false if not found or cannot delete</returns>
        Task<bool> DeleteAsync(Guid id, Guid userId);

        /// <summary>
        /// Duplicate an existing email template
        /// </summary>
        /// <param name="id">Source template ID</param>
        /// <param name="duplicateDto">Duplication data</param>
        /// <param name="userId">ID of the user creating the duplicate</param>
        /// <returns>Duplicated email template DTO or null if source not found</returns>
        Task<EmailTemplateDto?> DuplicateAsync(Guid id, DuplicateEmailTemplateDto duplicateDto, Guid userId);

        #endregion

        #region Template Rendering and Preview

        /// <summary>
        /// Render an email template with provided data
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="data">Data to merge with template</param>
        /// <returns>Rendered template or null if template not found</returns>
        Task<RenderedEmailTemplateDto?> RenderTemplateAsync(Guid templateId, Dictionary<string, object> data);

        /// <summary>
        /// Render an email template by name with provided data
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <param name="data">Data to merge with template</param>
        /// <returns>Rendered template or null if template not found</returns>
        Task<RenderedEmailTemplateDto?> RenderTemplateByNameAsync(string templateName, Dictionary<string, object> data);

        /// <summary>
        /// Preview an email template with sample data
        /// </summary>
        /// <param name="previewDto">Preview data</param>
        /// <returns>Preview result</returns>
        Task<RenderedEmailTemplateDto> PreviewTemplateAsync(EmailTemplatePreviewDto previewDto);

        /// <summary>
        /// Validate template syntax and variables
        /// </summary>
        /// <param name="validationDto">Validation data</param>
        /// <returns>Validation result</returns>
        Task<EmailTemplateValidationDto> ValidateTemplateAsync(string htmlContent, string subject);

        #endregion

        #region Template Management

        /// <summary>
        /// Get templates by type
        /// </summary>
        /// <param name="templateType">Template type</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of templates of the specified type</returns>
        Task<List<EmailTemplateDto>> GetByTypeAsync(string templateType, bool includeInactive = false);

        /// <summary>
        /// Get templates by category
        /// </summary>
        /// <param name="category">Template category</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of templates in the specified category</returns>
        Task<List<EmailTemplateDto>> GetByCategoryAsync(string category, bool includeInactive = false);

        /// <summary>
        /// Set template as default for its type
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="userId">ID of the user making the change</param>
        /// <returns>True if set successfully, false if not found or already default</returns>
        Task<bool> SetAsDefaultAsync(Guid id, Guid userId);

        /// <summary>
        /// Toggle template active status
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="userId">ID of the user making the change</param>
        /// <returns>Updated template DTO or null if not found</returns>
        Task<EmailTemplateDto?> ToggleActiveStatusAsync(Guid id, Guid userId);

        /// <summary>
        /// Get template usage statistics
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Template statistics or null if not found</returns>
        Task<EmailTemplateStatsDto?> GetTemplateStatsAsync(Guid id);

        /// <summary>
        /// Record template usage
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Task</returns>
        Task RecordTemplateUsageAsync(Guid templateId);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Perform bulk operations on multiple templates
        /// </summary>
        /// <param name="operationDto">Bulk operation data</param>
        /// <param name="userId">ID of the user performing the operation</param>
        /// <returns>Number of templates affected</returns>
        Task<int> BulkOperationAsync(BulkEmailTemplateOperationDto operationDto, Guid userId);

        /// <summary>
        /// Export templates
        /// </summary>
        /// <param name="templateIds">Template IDs to export (null for all)</param>
        /// <returns>Export data</returns>
        Task<EmailTemplateExportDto> ExportTemplatesAsync(List<Guid>? templateIds = null);

        /// <summary>
        /// Import templates
        /// </summary>
        /// <param name="importDto">Import data</param>
        /// <param name="userId">ID of the user performing the import</param>
        /// <returns>Import result with success/failure details</returns>
        Task<EmailTemplateImportDto> ImportTemplatesAsync(EmailTemplateImportDto importDto, Guid userId);

        #endregion

        #region System Template Management

        /// <summary>
        /// Create default system templates
        /// </summary>
        /// <param name="userId">ID of the admin user creating the templates</param>
        /// <returns>Number of templates created</returns>
        Task<int> CreateDefaultSystemTemplatesAsync(Guid userId);

        /// <summary>
        /// Get all system templates
        /// </summary>
        /// <returns>List of system templates</returns>
        Task<List<EmailTemplateDto>> GetSystemTemplatesAsync();

        /// <summary>
        /// Reset system template to default
        /// </summary>
        /// <param name="templateName">System template name</param>
        /// <param name="userId">ID of the user performing the reset</param>
        /// <returns>True if reset successfully, false if not found or not a system template</returns>
        Task<bool> ResetSystemTemplateAsync(string templateName, Guid userId);

        #endregion
    }
}
