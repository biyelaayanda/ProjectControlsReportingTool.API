using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectControlsReportingTool.API.Business.Interfaces
{
    public interface IUserService
    {
    Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto);
    Task<AuthResponseDto> AuthenticateUserAsync(LoginDto dto);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<AuthResponseDto> UpdateUserAsync(Guid id, UpdateProfileDto dto);
    Task<AuthResponseDto> DeactivateUserAsync(Guid id);
    }
}
