using Microsoft.EntityFrameworkCore;
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
            return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
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

        public async Task<IEnumerable<User>> GetByDepartmentAsync(Department department)
        {
            return await _dbSet
                .Where(u => u.Department == department && u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await _dbSet
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetLineManagersAsync()
        {
            return await GetByRoleAsync(UserRole.LineManager);
        }

        public async Task<IEnumerable<User>> GetExecutivesAsync()
        {
            return await GetByRoleAsync(UserRole.Executive);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            return VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
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

        public async Task<User> CreateUserAsync(User user, string password)
        {
            // Hash password
            var (hash, salt) = HashPassword(password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;

            return await AddAsync(user);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            if (!VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
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
