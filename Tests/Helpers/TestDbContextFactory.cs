using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Tests.Helpers
{
    /// <summary>
    /// Factory class for creating test database contexts with sample data
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Creates an in-memory SQLite database context for testing (no server required)
        /// Uses in-memory SQLite to avoid file cleanup issues
        /// </summary>
        public static ApplicationDbContext CreateInMemoryContext(string? databaseName = null)
        {
            databaseName ??= $"TestDb_{Guid.NewGuid()}";
            
            // Use in-memory SQLite to avoid file cleanup issues
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"Data Source={databaseName};Mode=Memory;Cache=Shared")
                .EnableSensitiveDataLogging()
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            
            return context;
        }

        /// <summary>
        /// Creates an in-memory database context with sample test data
        /// </summary>
        public static ApplicationDbContext CreateContextWithSampleData(string? databaseName = null)
        {
            var context = CreateInMemoryContext(databaseName);
            SeedTestData(context);
            return context;
        }

        /// <summary>
        /// Seeds the database context with comprehensive test data
        /// </summary>
        public static void SeedTestData(ApplicationDbContext context)
        {
            // Clear any existing data
            context.Users.RemoveRange(context.Users);
            context.Reports.RemoveRange(context.Reports);
            context.ReportTemplates.RemoveRange(context.ReportTemplates);
            context.AuditLogs.RemoveRange(context.AuditLogs);

            // Add test users
            var users = new[]
            {
                new User
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@test.com",
                    PasswordHash = "$2a$11$5M7.9Sp8ZZ9YdJ9J9J9J9OeGHqLqGvJqGvJqGvJqGvJqGvJqGvJqG", // "password123"
                    Role = UserRole.GeneralStaff,
                    Department = Department.ProjectSupport,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-30),
                    LastLoginDate = DateTime.UtcNow.AddDays(-1)
                },
                new User
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    FirstName = "Jane",
                    LastName = "Manager",
                    Email = "jane.manager@test.com",
                    PasswordHash = "$2a$11$5M7.9Sp8ZZ9YdJ9J9J9J9OeGHqLqGvJqGvJqGvJqGvJqGvJqGvJqG",
                    Role = UserRole.LineManager,
                    Department = Department.DocManagement,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-60),
                    LastLoginDate = DateTime.UtcNow.AddHours(-2)
                },
                new User
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    FirstName = "General",
                    LastName = "Manager",
                    Email = "gm@test.com",
                    PasswordHash = "$2a$11$5M7.9Sp8ZZ9YdJ9J9J9J9OeGHqLqGvJqGvJqGvJqGvJqGvJqGvJqG",
                    Role = UserRole.GM,
                    Department = Department.QS,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-90),
                    LastLoginDate = DateTime.UtcNow.AddHours(-1)
                }
            };

            context.Users.AddRange(users);

            // Add test report templates
            var reportTemplates = new[]
            {
                new ReportTemplate
                {
                    Id = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    Name = "Weekly Progress Report",
                    Description = "Standard weekly progress report template",
                    Type = "Weekly",
                    ContentTemplate = "<h1>Weekly Progress Report</h1><p>Progress details...</p>",
                    DefaultPriority = "Medium",
                    DefaultDepartment = Department.ProjectSupport,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-45),
                    CreatedBy = users[0].Id
                },
                new ReportTemplate
                {
                    Id = new Guid("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                    Name = "Monthly Summary Report",
                    Description = "Monthly summary report template",
                    Type = "Monthly",
                    ContentTemplate = "<h1>Monthly Summary Report</h1><p>Summary details...</p>",
                    DefaultPriority = "High",
                    DefaultDepartment = Department.DocManagement,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-30),
                    CreatedBy = users[1].Id
                }
            };

            context.ReportTemplates.AddRange(reportTemplates);

            // Add test reports
            var reports = new[]
            {
                new Report
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    Title = "Weekly Progress Report - Week 1",
                    Description = "First weekly progress report",
                    Content = "<h1>Progress Report</h1><p>Good progress made this week</p>",
                    Type = "Weekly",
                    Priority = "Medium",
                    Status = ReportStatus.Draft,
                    Department = Department.ProjectSupport,
                    CreatedBy = users[0].Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-7),
                    DueDate = DateTime.UtcNow.AddDays(7),
                    TemplateId = reportTemplates[0].Id
                },
                new Report
                {
                    Id = new Guid("55555555-5555-5555-5555-555555555555"),
                    Title = "Monthly Summary - January",
                    Description = "January monthly summary report",
                    Content = "<h1>Monthly Summary</h1><p>January was a productive month</p>",
                    Type = "Monthly",
                    Priority = "High",
                    Status = ReportStatus.Submitted,
                    Department = Department.DocManagement,
                    CreatedBy = users[0].Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-14),
                    SubmittedDate = DateTime.UtcNow.AddDays(-5),
                    DueDate = DateTime.UtcNow.AddDays(14),
                    TemplateId = reportTemplates[1].Id
                },
                new Report
                {
                    Id = new Guid("66666666-6666-6666-6666-666666666666"),
                    Title = "Completed Ad-hoc Report",
                    Description = "Special ad-hoc report that has been completed",
                    Content = "<h1>Ad-hoc Report</h1><p>Special requirements completed</p>",
                    Type = "AdHoc",
                    Priority = "Low",
                    Status = ReportStatus.Completed,
                    Department = Department.QS,
                    CreatedBy = users[1].Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-21),
                    SubmittedDate = DateTime.UtcNow.AddDays(-10),
                    CompletedDate = DateTime.UtcNow.AddDays(-3),
                    DueDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            context.Reports.AddRange(reports);

            // Add test audit logs
            var auditLogs = new[]
            {
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Action = AuditAction.Created,
                    ReportId = reports[0].Id,
                    Details = "Created new weekly progress report",
                    Timestamp = DateTime.UtcNow.AddDays(-7),
                    IpAddress = "127.0.0.1"
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Action = AuditAction.Submitted,
                    ReportId = reports[1].Id,
                    Details = "Submitted monthly summary report for review",
                    Timestamp = DateTime.UtcNow.AddDays(-5),
                    IpAddress = "127.0.0.1"
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Action = AuditAction.Approved,
                    ReportId = reports[2].Id,
                    Details = "Approved and completed ad-hoc report",
                    Timestamp = DateTime.UtcNow.AddDays(-3),
                    IpAddress = "127.0.0.1"
                }
            };

            context.AuditLogs.AddRange(auditLogs);

            // Save all changes
            context.SaveChanges();
        }

        /// <summary>
        /// Creates a minimal context with just essential data for testing
        /// </summary>
        public static ApplicationDbContext CreateMinimalContext(string? databaseName = null)
        {
            var context = CreateInMemoryContext(databaseName);
            
            // Add minimal test data - just one user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Role = UserRole.GeneralStaff,
                Department = Department.ProjectSupport,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.SaveChanges();
            
            return context;
        }

        /// <summary>
        /// Gets a test user by role for testing
        /// </summary>
        public static User GetTestUserByRole(ApplicationDbContext context, UserRole role)
        {
            return context.Users.First(u => u.Role == role);
        }

        /// <summary>
        /// Gets a test report by status for testing
        /// </summary>
        public static Report GetTestReportByStatus(ApplicationDbContext context, ReportStatus status)
        {
            return context.Reports.First(r => r.Status == status);
        }

        /// <summary>
        /// Creates a simple test report for testing
        /// </summary>
        public static Report CreateTestReport(Guid userId, ReportStatus status = ReportStatus.Draft)
        {
            return new Report
            {
                Id = Guid.NewGuid(),
                Title = $"Test Report - {status}",
                Description = $"Test report in {status} status",
                Content = "<h1>Test Report</h1><p>This is a test report.</p>",
                Type = "Test",
                Priority = "Medium",
                Status = status,
                Department = Department.ProjectSupport,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7)
            };
        }

        /// <summary>
        /// Creates a simple test user for testing
        /// </summary>
        public static User CreateTestUser(UserRole role = UserRole.GeneralStaff, Department department = Department.ProjectSupport)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Email = $"test.{role.ToString().ToLower()}@example.com",
                PasswordHash = "hashedpassword",
                Role = role,
                Department = department,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
        }
    }
}
