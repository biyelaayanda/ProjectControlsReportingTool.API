using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;

namespace ProjectControlsReportingTool.API.Repositories
{
    /// <summary>
    /// Repository implementation for email template data access operations
    /// Provides efficient data layer methods for managing email templates with caching
    /// </summary>
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public EmailTemplateRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        #region CRUD Operations

        public async Task<List<EmailTemplate>> GetAllAsync(EmailTemplateSearchDto? searchDto = null)
        {
            var cacheKey = $"email_templates_all_{searchDto?.GetHashCode() ?? 0}";
            
            if (_cache.TryGetValue(cacheKey, out List<EmailTemplate>? cachedTemplates))
            {
                return cachedTemplates!;
            }

            var query = _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .AsQueryable();

            if (searchDto != null)
            {
                query = ApplySearchFilters(query, searchDto);
            }

            var templates = await query
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _cache.Set(cacheKey, templates, _cacheExpiration);
            return templates;
        }

        public async Task<EmailTemplate?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"email_template_{id}";
            
            if (_cache.TryGetValue(cacheKey, out EmailTemplate? cachedTemplate))
            {
                return cachedTemplate;
            }

            var template = await _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template != null)
            {
                _cache.Set(cacheKey, template, _cacheExpiration);
            }

            return template;
        }

        public async Task<EmailTemplate?> GetByNameAsync(string name)
        {
            var cacheKey = $"email_template_name_{name.ToLowerInvariant()}";
            
            if (_cache.TryGetValue(cacheKey, out EmailTemplate? cachedTemplate))
            {
                return cachedTemplate;
            }

            var template = await _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

            if (template != null)
            {
                _cache.Set(cacheKey, template, _cacheExpiration);
            }

            return template;
        }

        public async Task<EmailTemplate> CreateAsync(EmailTemplate template)
        {
            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Clear relevant caches
            ClearRelatedCaches();

            return template;
        }

        public async Task<EmailTemplate> UpdateAsync(EmailTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            template.Version++;

            _context.EmailTemplates.Update(template);
            await _context.SaveChangesAsync();

            // Clear relevant caches
            ClearRelatedCaches();
            _cache.Remove($"email_template_{template.Id}");
            _cache.Remove($"email_template_name_{template.Name.ToLowerInvariant()}");

            return template;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null || template.IsSystemTemplate)
            {
                return false;
            }

            _context.EmailTemplates.Remove(template);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // Clear relevant caches
                ClearRelatedCaches();
                _cache.Remove($"email_template_{id}");
                _cache.Remove($"email_template_name_{template.Name.ToLowerInvariant()}");
            }

            return result;
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var query = _context.EmailTemplates.AsQueryable();
            
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync(t => t.Name.ToLower() == name.ToLower());
        }

        #endregion

        #region Query Operations

        public async Task<List<EmailTemplate>> GetByTypeAsync(string templateType, bool includeInactive = false)
        {
            var cacheKey = $"email_templates_type_{templateType}_{includeInactive}";
            
            if (_cache.TryGetValue(cacheKey, out List<EmailTemplate>? cachedTemplates))
            {
                return cachedTemplates!;
            }

            var query = _context.EmailTemplates
                .Include(t => t.Creator)
                .Where(t => t.TemplateType == templateType);

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            var templates = await query
                .OrderBy(t => t.Name)
                .ToListAsync();

            _cache.Set(cacheKey, templates, _cacheExpiration);
            return templates;
        }

        public async Task<List<EmailTemplate>> GetByCategoryAsync(string category, bool includeInactive = false)
        {
            var cacheKey = $"email_templates_category_{category}_{includeInactive}";
            
            if (_cache.TryGetValue(cacheKey, out List<EmailTemplate>? cachedTemplates))
            {
                return cachedTemplates!;
            }

            var query = _context.EmailTemplates
                .Include(t => t.Creator)
                .Where(t => t.Category == category);

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            var templates = await query
                .OrderBy(t => t.Name)
                .ToListAsync();

            _cache.Set(cacheKey, templates, _cacheExpiration);
            return templates;
        }

        public async Task<EmailTemplate?> GetDefaultByTypeAsync(string templateType)
        {
            var cacheKey = $"email_template_default_{templateType}";
            
            if (_cache.TryGetValue(cacheKey, out EmailTemplate? cachedTemplate))
            {
                return cachedTemplate;
            }

            var template = await _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .FirstOrDefaultAsync(t => t.TemplateType == templateType && t.IsDefault && t.IsActive);

            if (template != null)
            {
                _cache.Set(cacheKey, template, _cacheExpiration);
            }

            return template;
        }

        public async Task<List<EmailTemplate>> GetSystemTemplatesAsync()
        {
            const string cacheKey = "email_templates_system";
            
            if (_cache.TryGetValue(cacheKey, out List<EmailTemplate>? cachedTemplates))
            {
                return cachedTemplates!;
            }

            var templates = await _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .Where(t => t.IsSystemTemplate)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _cache.Set(cacheKey, templates, _cacheExpiration);
            return templates;
        }

        public async Task<List<EmailTemplate>> GetByCreatorAsync(Guid userId)
        {
            var cacheKey = $"email_templates_creator_{userId}";
            
            if (_cache.TryGetValue(cacheKey, out List<EmailTemplate>? cachedTemplates))
            {
                return cachedTemplates!;
            }

            var templates = await _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .Where(t => t.CreatedBy == userId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            _cache.Set(cacheKey, templates, TimeSpan.FromMinutes(5));
            return templates;
        }

        public async Task<List<EmailTemplate>> GetMostUsedAsync(int count = 10)
        {
            const string cacheKey = "email_templates_most_used";
            
            if (_cache.TryGetValue(cacheKey, out List<EmailTemplate>? cachedTemplates))
            {
                return cachedTemplates!;
            }

            var templates = await _context.EmailTemplates
                .Include(t => t.Creator)
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.UsageCount)
                .Take(count)
                .ToListAsync();

            _cache.Set(cacheKey, templates, TimeSpan.FromMinutes(30));
            return templates;
        }

        public async Task<List<EmailTemplate>> GetRecentlyUsedAsync(int count = 10)
        {
            const string cacheKey = "email_templates_recently_used";
            
            if (_cache.TryGetValue(cacheKey, out List<EmailTemplate>? cachedTemplates))
            {
                return cachedTemplates!;
            }

            var templates = await _context.EmailTemplates
                .Include(t => t.Creator)
                .Where(t => t.IsActive && t.UsageCount > 0)
                .OrderByDescending(t => t.LastUsed)
                .Take(count)
                .ToListAsync();

            _cache.Set(cacheKey, templates, TimeSpan.FromMinutes(10));
            return templates;
        }

        #endregion

        #region Statistics and Analytics

        public async Task<int> GetTotalCountAsync()
        {
            const string cacheKey = "email_templates_total_count";
            
            if (_cache.TryGetValue(cacheKey, out int cachedCount))
            {
                return cachedCount;
            }

            var count = await _context.EmailTemplates.CountAsync();
            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(30));
            return count;
        }

        public async Task<int> GetActiveCountAsync()
        {
            const string cacheKey = "email_templates_active_count";
            
            if (_cache.TryGetValue(cacheKey, out int cachedCount))
            {
                return cachedCount;
            }

            var count = await _context.EmailTemplates.CountAsync(t => t.IsActive);
            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(30));
            return count;
        }

        public async Task<int> GetCountByTypeAsync(string templateType)
        {
            var cacheKey = $"email_templates_count_type_{templateType}";
            
            if (_cache.TryGetValue(cacheKey, out int cachedCount))
            {
                return cachedCount;
            }

            var count = await _context.EmailTemplates.CountAsync(t => t.TemplateType == templateType);
            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(30));
            return count;
        }

        public async Task<(int UsageCount, DateTime LastUsed)> GetUsageStatsAsync(Guid templateId)
        {
            var template = await _context.EmailTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == templateId);

            return template != null 
                ? (template.UsageCount, template.LastUsed)
                : (0, DateTime.MinValue);
        }

        public async Task<Dictionary<DateTime, int>> GetCreationStatsAsync(DateTime startDate, DateTime endDate)
        {
            var templates = await _context.EmailTemplates
                .AsNoTracking()
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return templates.ToDictionary(x => x.Date, x => x.Count);
        }

        #endregion

        #region Bulk Operations

        public async Task<List<EmailTemplate>> GetByIdsAsync(List<Guid> ids)
        {
            return await _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .Where(t => ids.Contains(t.Id))
                .ToListAsync();
        }

        public async Task<int> BulkUpdateActiveStatusAsync(List<Guid> templateIds, bool isActive)
        {
            var templates = await _context.EmailTemplates
                .Where(t => templateIds.Contains(t.Id) && !t.IsSystemTemplate)
                .ToListAsync();

            foreach (var template in templates)
            {
                template.IsActive = isActive;
                template.UpdatedAt = DateTime.UtcNow;
            }

            var result = await _context.SaveChangesAsync();
            
            if (result > 0)
            {
                ClearRelatedCaches();
            }

            return templates.Count;
        }

        public async Task<int> BulkDeleteAsync(List<Guid> templateIds)
        {
            var templates = await _context.EmailTemplates
                .Where(t => templateIds.Contains(t.Id) && !t.IsSystemTemplate)
                .ToListAsync();

            _context.EmailTemplates.RemoveRange(templates);
            var result = await _context.SaveChangesAsync();
            
            if (result > 0)
            {
                ClearRelatedCaches();
            }

            return templates.Count;
        }

        public async Task<int> BulkUpdateCategoryAsync(List<Guid> templateIds, string category)
        {
            var templates = await _context.EmailTemplates
                .Where(t => templateIds.Contains(t.Id))
                .ToListAsync();

            foreach (var template in templates)
            {
                template.Category = category;
                template.UpdatedAt = DateTime.UtcNow;
            }

            var result = await _context.SaveChangesAsync();
            
            if (result > 0)
            {
                ClearRelatedCaches();
            }

            return templates.Count;
        }

        #endregion

        #region Template Management

        public async Task<bool> SetAsDefaultAsync(Guid templateId, string templateType)
        {
            // First, unset any existing default for this type
            var existingDefaults = await _context.EmailTemplates
                .Where(t => t.TemplateType == templateType && t.IsDefault)
                .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            // Set the new default
            var template = await _context.EmailTemplates.FindAsync(templateId);
            if (template == null || template.TemplateType != templateType)
            {
                return false;
            }

            template.IsDefault = true;
            template.UpdatedAt = DateTime.UtcNow;

            var result = await _context.SaveChangesAsync() > 0;
            
            if (result)
            {
                ClearRelatedCaches();
                _cache.Remove($"email_template_default_{templateType}");
            }

            return result;
        }

        public async Task IncrementUsageAsync(Guid templateId)
        {
            var template = await _context.EmailTemplates.FindAsync(templateId);
            if (template != null)
            {
                template.IncrementUsage();
                await _context.SaveChangesAsync();

                // Clear usage-related caches
                _cache.Remove("email_templates_most_used");
                _cache.Remove("email_templates_recently_used");
            }
        }

        public async Task<int> GetNextVersionAsync(string templateName)
        {
            var maxVersion = await _context.EmailTemplates
                .Where(t => t.Name == templateName)
                .MaxAsync(t => (int?)t.Version) ?? 0;

            return maxVersion + 1;
        }

        public async Task<bool> CanDeleteAsync(Guid templateId)
        {
            var template = await _context.EmailTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null || template.IsSystemTemplate)
            {
                return false;
            }

            // Add additional checks here if needed (e.g., if template is referenced elsewhere)
            return true;
        }

        #endregion

        #region Search and Filtering

        public async Task<List<EmailTemplate>> SearchAsync(string searchText, bool includeInactive = false)
        {
            var query = _context.EmailTemplates
                .Include(t => t.Creator)
                .Where(t => t.Name.Contains(searchText) || 
                           t.Description.Contains(searchText) || 
                           t.Subject.Contains(searchText) ||
                           t.HtmlContent.Contains(searchText) ||
                           t.PlainTextContent.Contains(searchText));

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            return await query
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<(List<EmailTemplate> Templates, int TotalCount)> GetFilteredAsync(EmailTemplateSearchDto searchDto)
        {
            var query = _context.EmailTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastUpdater)
                .AsQueryable();

            query = ApplySearchFilters(query, searchDto);

            var totalCount = await query.CountAsync();

            // Apply pagination
            if (searchDto.PageSize > 0 && searchDto.Page > 0)
            {
                query = query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize);
            }

            // Apply sorting
            query = ApplySorting(query, searchDto.SortBy, searchDto.SortDirection == "desc");

            var templates = await query.ToListAsync();

            return (templates, totalCount);
        }

        #endregion

        #region Private Helper Methods

        private static IQueryable<EmailTemplate> ApplySearchFilters(IQueryable<EmailTemplate> query, EmailTemplateSearchDto searchDto)
        {
            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var searchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Name.ToLower().Contains(searchTerm) ||
                    t.Description.ToLower().Contains(searchTerm) ||
                    t.Subject.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.TemplateType))
            {
                query = query.Where(t => t.TemplateType == searchDto.TemplateType);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Category))
            {
                query = query.Where(t => t.Category == searchDto.Category);
            }

            if (searchDto.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == searchDto.IsActive.Value);
            }

            if (searchDto.IsSystemTemplate.HasValue)
            {
                query = query.Where(t => t.IsSystemTemplate == searchDto.IsSystemTemplate.Value);
            }

            if (searchDto.IsDefault.HasValue)
            {
                query = query.Where(t => t.IsDefault == searchDto.IsDefault.Value);
            }

            if (searchDto.CreatedBy.HasValue)
            {
                query = query.Where(t => t.CreatedBy == searchDto.CreatedBy.Value);
            }

            if (searchDto.CreatedFrom.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= searchDto.CreatedFrom.Value);
            }

            if (searchDto.CreatedTo.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= searchDto.CreatedTo.Value);
            }

            if (searchDto.LastUsedFrom.HasValue)
            {
                query = query.Where(t => t.LastUsed >= searchDto.LastUsedFrom.Value);
            }

            if (searchDto.LastUsedTo.HasValue)
            {
                query = query.Where(t => t.LastUsed <= searchDto.LastUsedTo.Value);
            }

            if (searchDto.MinUsageCount.HasValue)
            {
                query = query.Where(t => t.UsageCount >= searchDto.MinUsageCount.Value);
            }

            if (searchDto.MaxUsageCount.HasValue)
            {
                query = query.Where(t => t.UsageCount <= searchDto.MaxUsageCount.Value);
            }

            return query;
        }

        private static IQueryable<EmailTemplate> ApplySorting(IQueryable<EmailTemplate> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query.OrderBy(t => t.Category).ThenBy(t => t.Name);
            }

            return sortBy.ToLowerInvariant() switch
            {
                "name" => sortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "category" => sortDescending ? query.OrderByDescending(t => t.Category) : query.OrderBy(t => t.Category),
                "templatetype" => sortDescending ? query.OrderByDescending(t => t.TemplateType) : query.OrderBy(t => t.TemplateType),
                "createdat" => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                "updatedat" => sortDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
                "usagecount" => sortDescending ? query.OrderByDescending(t => t.UsageCount) : query.OrderBy(t => t.UsageCount),
                "lastused" => sortDescending ? query.OrderByDescending(t => t.LastUsed) : query.OrderBy(t => t.LastUsed),
                _ => query.OrderBy(t => t.Category).ThenBy(t => t.Name)
            };
        }

        private void ClearRelatedCaches()
        {
            // Clear all template-related caches
            var cacheKeys = new[]
            {
                "email_templates_total_count",
                "email_templates_active_count",
                "email_templates_system",
                "email_templates_most_used",
                "email_templates_recently_used"
            };

            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }

            // Clear pattern-based caches (this is a simplified approach)
            // In a real application, you might want to use a more sophisticated cache invalidation strategy
        }

        #endregion
    }
}
