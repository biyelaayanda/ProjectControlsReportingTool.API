using ProjectControlsReportingTool.API.Models.DTOs;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<ServiceResultDto> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
        Task<ServiceResultDto> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto);
        Task<UserDto?> GetCurrentUserAsync(Guid userId);
    }
}
