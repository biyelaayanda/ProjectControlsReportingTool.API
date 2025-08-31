using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Repositories
{
    /// <summary>
    /// Repository interface for email template data access operations
    /// Provides data layer methods for managing email templates
    /// </summary>
    public interface IEmailTemplateRepository
    {
        #region CRUD Operations

        /// <summary>
        /// Get all email templates with optional filtering
        /// </summary>
        /// <param name="searchDto">Optional search criteria</param>
        /// <returns>List of email templates</returns>
        Task<List<EmailTemplate>> GetAllAsync(EmailTemplateSearchDto? searchDto = null);

        /// <summary>
        /// Get email template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Email template or null if not found</returns>
        Task<EmailTemplate?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get email template by name
        /// </summary>
        /// <param name="name">Template name</param>
        /// <returns>Email template or null if not found</returns>
        Task<EmailTemplate?> GetByNameAsync(string name);

        /// <summary>
        /// Create a new email template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>Created template</returns>
        Task<EmailTemplate> CreateAsync(EmailTemplate template);

        /// <summary>
        /// Update an existing email template
        /// </summary>
        /// <param name="template">Template to update</param>
        /// <returns>Updated template</returns>
        Task<EmailTemplate> UpdateAsync(EmailTemplate template);

        /// <summary>
        /// Delete an email template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Check if template exists by name
        /// </summary>
        /// <param name="name">Template name</param>
        /// <param name="excludeId">ID to exclude from check (for updates)</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get templates by type
        /// </summary>
        /// <param name="templateType">Template type</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of templates of the specified type</returns>
        Task<List<EmailTemplate>> GetByTypeAsync(string templateType, bool includeInactive = false);

        /// <summary>
        /// Get templates by category
        /// </summary>
        /// <param name="category">Template category</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of templates in the specified category</returns>
        Task<List<EmailTemplate>> GetByCategoryAsync(string category, bool includeInactive = false);

        /// <summary>
        /// Get default template for a specific type
        /// </summary>
        /// <param name="templateType">Template type</param>
        /// <returns>Default template or null if not found</returns>
        Task<EmailTemplate?> GetDefaultByTypeAsync(string templateType);

        /// <summary>
        /// Get system templates
        /// </summary>
        /// <returns>List of system templates</returns>
        Task<List<EmailTemplate>> GetSystemTemplatesAsync();

        /// <summary>
        /// Get templates created by user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of templates created by the user</returns>
        Task<List<EmailTemplate>> GetByCreatorAsync(Guid userId);

        /// <summary>
        /// Get most used templates
        /// </summary>
        /// <param name="count">Number of templates to return</param>
        /// <returns>List of most used templates</returns>
        Task<List<EmailTemplate>> GetMostUsedAsync(int count = 10);

        /// <summary>
        /// Get recently used templates
        /// </summary>
        /// <param name="count">Number of templates to return</param>
        /// <returns>List of recently used templates</returns>
        Task<List<EmailTemplate>> GetRecentlyUsedAsync(int count = 10);

        #endregion

        #region Statistics and Analytics

        /// <summary>
        /// Get total template count
        /// </summary>
        /// <returns>Total number of templates</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Get active template count
        /// </summary>
        /// <returns>Number of active templates</returns>
        Task<int> GetActiveCountAsync();

        /// <summary>
        /// Get template count by type
        /// </summary>
        /// <param name="templateType">Template type</param>
        /// <returns>Number of templates of the specified type</returns>
        Task<int> GetCountByTypeAsync(string templateType);

        /// <summary>
        /// Get template usage statistics
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Usage statistics</returns>
        Task<(int UsageCount, DateTime LastUsed)> GetUsageStatsAsync(Guid templateId);

        /// <summary>
        /// Get template creation statistics for a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Dictionary of dates and creation counts</returns>
        Task<Dictionary<DateTime, int>> GetCreationStatsAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Get templates by multiple IDs
        /// </summary>
        /// <param name="ids">Template IDs</param>
        /// <returns>List of templates</returns>
        Task<List<EmailTemplate>> GetByIdsAsync(List<Guid> ids);

        /// <summary>
        /// Bulk update template active status
        /// </summary>
        /// <param name="templateIds">Template IDs</param>
        /// <param name="isActive">New active status</param>
        /// <returns>Number of templates updated</returns>
        Task<int> BulkUpdateActiveStatusAsync(List<Guid> templateIds, bool isActive);

        /// <summary>
        /// Bulk delete templates
        /// </summary>
        /// <param name="templateIds">Template IDs</param>
        /// <returns>Number of templates deleted</returns>
        Task<int> BulkDeleteAsync(List<Guid> templateIds);

        /// <summary>
        /// Bulk update template category
        /// </summary>
        /// <param name="templateIds">Template IDs</param>
        /// <param name="category">New category</param>
        /// <returns>Number of templates updated</returns>
        Task<int> BulkUpdateCategoryAsync(List<Guid> templateIds, string category);

        #endregion

        #region Template Management

        /// <summary>
        /// Set template as default for its type (unsets other defaults)
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="templateType">Template type</param>
        /// <returns>True if set successfully</returns>
        Task<bool> SetAsDefaultAsync(Guid templateId, string templateType);

        /// <summary>
        /// Increment template usage count
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Task</returns>
        Task IncrementUsageAsync(Guid templateId);

        /// <summary>
        /// Get next version number for template
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <returns>Next version number</returns>
        Task<int> GetNextVersionAsync(string templateName);

        /// <summary>
        /// Check if template can be deleted (not system template, not in use, etc.)
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>True if can be deleted, false otherwise</returns>
        Task<bool> CanDeleteAsync(Guid templateId);

        #endregion

        #region Search and Filtering

        /// <summary>
        /// Search templates by text in name, description, or content
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="includeInactive">Include inactive templates</param>
        /// <returns>List of matching templates</returns>
        Task<List<EmailTemplate>> SearchAsync(string searchText, bool includeInactive = false);

        /// <summary>
        /// Get filtered templates with pagination
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Paginated result</returns>
        Task<(List<EmailTemplate> Templates, int TotalCount)> GetFilteredAsync(EmailTemplateSearchDto searchDto);

        #endregion
    }
}
