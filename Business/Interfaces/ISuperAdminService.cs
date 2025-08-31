using ProjectControlsReportingTool.API.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public interface ISuperAdminService
    {
        // User Management
        Task<PagedResultDto<SuperAdminUserDetailDto>> GetUsersAsync(SuperAdminUserFiltersDto filters);
        Task<SuperAdminUserDetailDto?> GetUserByIdAsync(Guid userId);
        Task<SuperAdminUserDetailDto> CreateUserAsync(SuperAdminCreateUserDto createUserDto, Guid currentUserId);
        Task<SuperAdminUserDetailDto?> UpdateUserAsync(Guid userId, SuperAdminUpdateUserDto updateUserDto, Guid currentUserId);
        Task<bool> DeleteUserAsync(Guid userId, string reason, bool permanent, Guid currentUserId);
        Task<SuperAdminPasswordResetResultDto?> ResetUserPasswordAsync(Guid userId, bool sendEmail, Guid currentUserId);
        Task<SuperAdminUserDetailDto?> ToggleUserStatusAsync(Guid userId, string reason, Guid currentUserId);

        // Bulk Operations
        Task<SuperAdminBulkOperationResultDto> BulkCreateUsersAsync(SuperAdminBulkCreateUsersDto bulkCreateDto, Guid currentUserId);
        Task<SuperAdminBulkOperationResultDto> BulkUpdateUsersAsync(SuperAdminBulkUpdateUsersDto bulkUpdateDto, Guid currentUserId);
        Task<SuperAdminBulkOperationResultDto> BulkDeleteUsersAsync(SuperAdminBulkDeleteUsersDto bulkDeleteDto, Guid currentUserId);

        // Import/Export
        Task<SuperAdminImportResultDto> ImportUsersAsync(IFormFile file, bool preview, bool sendWelcomeEmails, Guid currentUserId);
        Task<byte[]> ExportUsersAsync(SuperAdminUserFiltersDto filters);

        // Audit Reports
        Task<SuperAdminDepartmentAuditReportDto> GenerateDepartmentReportAsync(SuperAdminDepartmentReportParamsDto parameters, Guid currentUserId);
        Task<SuperAdminUserAuditReportDto> GenerateUserReportAsync(SuperAdminUserReportParamsDto parameters, Guid currentUserId);

        // System Administration
        Task<SuperAdminSystemHealthReportDto> GetSystemHealthAsync();
        Task<PagedResultDto<SuperAdminAuditEntryDto>> GetAuditTrailAsync(SuperAdminAuditTrailFiltersDto filters);
        Task<object> GetDashboardStatsAsync();

        // Session Management
        Task LogAdminSessionAsync(Guid adminUserId, string action, string ipAddress);
        Task<bool> ValidateAdminPermissionsAsync(Guid adminUserId, string operation);
    }
}
