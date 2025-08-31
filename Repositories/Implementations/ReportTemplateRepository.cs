using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using ProjectControlsReportingTool.API.Repositories.Base;
using ProjectControlsReportingTool.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjectControlsReportingTool.API.Repositories.Implementations
{
    public class ReportTemplateRepository : BaseRepository<ReportTemplate>, IReportTemplateRepository
    {
        public ReportTemplateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ReportTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.ReportTemplates
                .Where(t => t.IsActive)
                .Include(t => t.Creator)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReportTemplate>> GetTemplatesByDepartmentAsync(Department department)
        {
            return await _context.ReportTemplates
                .Where(t => t.IsActive && (t.DefaultDepartment == department || t.DefaultDepartment == null))
                .Include(t => t.Creator)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReportTemplate>> GetTemplatesByTypeAsync(string type)
        {
            return await _context.ReportTemplates
                .Where(t => t.IsActive && t.Type == type)
                .Include(t => t.Creator)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReportTemplate>> GetSystemTemplatesAsync()
        {
            return await _context.ReportTemplates
                .Where(t => t.IsSystemTemplate && t.IsActive)
                .Include(t => t.Creator)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReportTemplate>> GetUserTemplatesAsync(Guid userId)
        {
            return await _context.ReportTemplates
                .Where(t => t.CreatedBy == userId)
                .Include(t => t.Creator)
                .OrderBy(t => t.IsActive ? 0 : 1)
                .ThenBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<PagedResultDto<ReportTemplate>> GetFilteredTemplatesAsync(ReportTemplateFilterDto filter)
        {
            var query = _context.ReportTemplates
                .Include(t => t.Creator)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(t => 
                    t.Name.Contains(filter.SearchTerm) ||
                    (t.Description != null && t.Description.Contains(filter.SearchTerm)) ||
                    (t.Tags != null && t.Tags.Contains(filter.SearchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(t => t.Type == filter.Type);
            }

            if (filter.Department.HasValue)
            {
                query = query.Where(t => t.DefaultDepartment == filter.Department.Value || t.DefaultDepartment == null);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == filter.IsActive.Value);
            }

            if (filter.IsSystemTemplate.HasValue)
            {
                query = query.Where(t => t.IsSystemTemplate == filter.IsSystemTemplate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Tag))
            {
                query = query.Where(t => t.Tags != null && t.Tags.Contains(filter.Tag));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "name":
                        query = filter.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name);
                        break;
                    case "createdate":
                        query = filter.SortDescending ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate);
                        break;
                    case "usage":
                        // For now, just sort by name if usage sorting is requested
                        query = query.OrderBy(t => t.Name);
                        break;
                    default:
                        query = query.OrderBy(t => t.SortOrder).ThenBy(t => t.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(t => t.SortOrder).ThenBy(t => t.Name);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResultDto<ReportTemplate>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasNext = filter.PageNumber < (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasPrevious = filter.PageNumber > 1
            };
        }

        public async Task<ReportTemplate?> GetTemplateWithUsageCountAsync(Guid id)
        {
            return await _context.ReportTemplates
                .Include(t => t.Creator)
                .Include(t => t.LastModifiedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<int> GetTemplateUsageCountAsync(Guid templateId)
        {
            return await _context.Reports
                .CountAsync(r => r.TemplateId == templateId);
        }

        public async Task<IEnumerable<ReportTemplate>> SearchTemplatesAsync(string searchTerm)
        {
            return await _context.ReportTemplates
                .Where(t => t.IsActive && 
                    (t.Name.Contains(searchTerm) ||
                     (t.Description != null && t.Description.Contains(searchTerm)) ||
                     (t.Tags != null && t.Tags.Contains(searchTerm))))
                .Include(t => t.Creator)
                .OrderBy(t => t.Name)
                .Take(20) // Limit search results
                .ToListAsync();
        }

        public async Task<bool> TemplateNameExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.ReportTemplates.Where(t => t.Name == name);
            
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CanDeleteTemplateAsync(Guid templateId)
        {
            var usageCount = await GetTemplateUsageCountAsync(templateId);
            var template = await _context.ReportTemplates.FindAsync(templateId);
            
            // Can't delete system templates or templates that are in use
            return template != null && !template.IsSystemTemplate && usageCount == 0;
        }

        public async Task<IEnumerable<string>> GetAllTemplateTypesAsync()
        {
            return await _context.ReportTemplates
                .Where(t => !string.IsNullOrEmpty(t.Type))
                .Select(t => t.Type!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAllTagsAsync()
        {
            var allTags = await _context.ReportTemplates
                .Where(t => !string.IsNullOrEmpty(t.Tags))
                .Select(t => t.Tags!)
                .ToListAsync();

            return allTags
                .SelectMany(tags => tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Distinct()
                .OrderBy(tag => tag)
                .ToList();
        }
    }
}
