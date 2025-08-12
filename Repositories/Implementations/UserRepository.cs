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
            var parameters = new[]
            {
                new SqlParameter("@Email", email)
            };

            var users = await _context.Users
                .FromSqlRaw("EXEC GetUserByEmail @Email", parameters)
                .ToListAsync();

            return users.FirstOrDefault();
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
            var parameters = new[]
            {
                new SqlParameter("@Email", email),
                new SqlParameter("@PasswordHash", passwordHash)
            };

            var users = await _context.Users
                .FromSqlRaw("EXEC AuthenticateUser @Email, @PasswordHash", parameters)
                .ToListAsync();

            return users.FirstOrDefault();
        }

        public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(Department department)
        {
            var parameters = new[]
            {
                new SqlParameter("@Department", (int)department)
            };

            return await _context.Users
                .FromSqlRaw("EXEC GetUsersByDepartment @Department", parameters)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            var parameters = new[]
            {
                new SqlParameter("@Role", (int)role)
            };

            return await _context.Users
                .FromSqlRaw("EXEC GetUsersByRole @Role", parameters)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var parameters = new[]
            {
                new SqlParameter("@Email", email),
                new SqlParameter("@Exists", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output }
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC CheckEmailExists @Email, @Exists OUTPUT", parameters);
            
            return (bool)parameters[1].Value;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", user.Id),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@FirstName", user.FirstName),
                new SqlParameter("@LastName", user.LastName),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@PasswordSalt", user.PasswordSalt),
                new SqlParameter("@Role", (int)user.Role),
                new SqlParameter("@Department", (int)user.Department),
                new SqlParameter("@IsActive", user.IsActive),
                new SqlParameter("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value),
                new SqlParameter("@JobTitle", (object)user.JobTitle ?? DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC CreateUser @Id, @Email, @FirstName, @LastName, @PasswordHash, @PasswordSalt, @Role, @Department, @IsActive, @PhoneNumber, @JobTitle", 
                parameters);

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
