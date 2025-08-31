using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> AuthenticateUserAsync(string email, string passwordHash);
        Task<IEnumerable<User>> GetUsersByDepartmentAsync(Department department);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task UpdateLastLoginAsync(Guid userId);
        Task<bool> UpdatePasswordAsync(User user, string newPassword);
    }
}
