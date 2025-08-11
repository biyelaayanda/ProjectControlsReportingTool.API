using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Interfaces;

namespace ProjectControlsReportingTool.API.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> AuthenticateUserAsync(string email, string passwordHash);
        Task<IEnumerable<User>> GetUsersByDepartmentAsync(Department department);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task<bool> UpdateLastLoginAsync(Guid userId);
    }
}
