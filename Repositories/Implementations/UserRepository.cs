using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Repositories.Base;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
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
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            // First get the user to retrieve their salt
            var user = await GetByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // Hash the provided password with the user's salt
            var passwordHash = HashPasswordWithSalt(password, user.PasswordSalt);
            
            // Check if the hashed password matches
            if (user.PasswordHash == passwordHash)
            {
                return user;
            }

            return null;
        }

        public async Task<User?> AuthenticateUserAsync(string email, string passwordHash)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash && u.IsActive);
        }

        public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(Department department)
        {
            return await _context.Users
                .Where(u => u.Department == department)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Use Entity Framework instead of stored procedure
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            // Generate salt and hash password
            user.PasswordSalt = GenerateSalt();
            user.PasswordHash = HashPasswordWithSalt(password, user.PasswordSalt);

            return await CreateUserAsync(user);
        }

        public async Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@NewRole", (int)newRole)
            };

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC UpdateUserRole @UserId, @NewRole", parameters);
            return result > 0;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            var result = await _context.Database.ExecuteSqlRawAsync("EXEC DeactivateUser @UserId", parameters);
            return result > 0;
        }

        public async Task UpdateLastLoginAsync(Guid userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LastLoginAt", DateTime.UtcNow)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC UpdateLastLogin @UserId, @LastLoginAt", parameters);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPasswordWithSalt(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashedBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
