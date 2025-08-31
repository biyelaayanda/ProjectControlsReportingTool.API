using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    public interface IUserService
    {
        // Existing methods
        Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto);
        Task<AuthResponseDto> AuthenticateUserAsync(LoginDto dto);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<AuthResponseDto> UpdateUserAsync(Guid id, UpdateProfileDto dto);
        Task<AuthResponseDto> DeactivateUserAsync(Guid id);

        // New bulk operations methods
        Task<BulkOperationResultDto> BulkAssignRoleAsync(BulkRoleAssignmentDto dto);
        Task<BulkOperationResultDto> BulkChangeDepartmentAsync(BulkDepartmentChangeDto dto);
        Task<BulkOperationResultDto> BulkActivateDeactivateAsync(BulkActivationDto dto);
        Task<BulkOperationResultDto> BulkImportUsersAsync(BulkUserImportDto dto);
        
        // Enhanced user management
        Task<PagedResultDto<UserDto>> GetFilteredUsersAsync(UserFilterDto filter);
        Task<AuthResponseDto> AdminUpdateUserAsync(Guid id, AdminUserUpdateDto dto);
        Task<AuthResponseDto> ResetUserPasswordAsync(Guid id, string newPassword);
        Task<bool> DeleteUserPermanentlyAsync(Guid id);
    }
}
