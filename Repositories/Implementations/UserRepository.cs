using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Base;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ProjectControlsReportingTool.API.Repositories.Implementations
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null || !user.IsActive)
                return null;

            if (VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return user;

            return null;
        }

        public async Task<User?> AuthenticateUserAsync(string email, string passwordHash)
        {
            return await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash && u.IsActive);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _context.Set<User>()
                .Where(u => u.Role == role && u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(Department department)
        {
            return await _context.Set<User>()
                .Where(u => u.Department == department && u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Set<User>()
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Set<User>()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;
            await AddAsync(user);
            return user;
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            var (hash, salt) = HashPassword(password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;

            await AddAsync(user);
            return user;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null || !VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
                return false;

            var (hash, salt) = HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            await UpdateAsync(user);
            return true;
        }

        public async Task UpdateLastLoginAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await UpdateAsync(user);
            }
        }

        public async Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.Role = newRole;
                await UpdateAsync(user);
                return true;
            }
            return false;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await UpdateAsync(user);
                return true;
            }
            return false;
        }

        public async Task<bool> ReactivateUserAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = true;
                await UpdateAsync(user);
                return true;
            }
            return false;
        }

        // Note: GetUsersByManagerAsync removed as User entity doesn't have ManagerId property
        
        public async Task<int> GetUserCountByDepartmentAsync(Department department)
        {
            return await _context.Set<User>()
                .CountAsync(u => u.Department == department && u.IsActive);
        }

        public async Task<User?> GetUserWithReportsAsync(Guid userId)
        {
            return await _context.Set<User>()
                .Include(u => u.CreatedReports)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            var (hash, salt) = HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            await UpdateAsync(user);
            return true;
        }

        #region Password Hashing Helpers

        private static (string Hash, string Salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(32);
            var hash = Convert.ToBase64String(hashBytes);

            return (hash, salt);
        }

        private static bool VerifyPassword(string password, string hash, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(32);
            var computedHash = Convert.ToBase64String(hashBytes);

            return hash == computedHash;
        }

        #endregion
    }
}
