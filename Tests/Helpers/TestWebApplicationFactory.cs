using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProjectControlsReportingTool.API.Data;

namespace ProjectControlsReportingTool.API.Tests.Helpers
{
    /// <summary>
    /// Custom WebApplicationFactory for integration testing
    /// Configures in-memory database and test services
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName;

        public TestWebApplicationFactory()
        {
            _databaseName = Guid.NewGuid().ToString();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Set the content root to the correct project directory
            var projectDir = Directory.GetCurrentDirectory();
            builder.UseContentRoot(projectDir);
            
            builder.ConfigureServices(services =>
            {
                // Remove ALL Entity Framework and database-related services to avoid conflicts
                var descriptorsToRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(ApplicationDbContext) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                    d.ImplementationType?.FullName?.Contains("SqlServer") == true ||
                    d.ImplementationType?.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using SQLite database for testing (no server required)
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={_databaseName}.db");
                    options.EnableSensitiveDataLogging();
                });

                // Disable logging during tests
                services.RemoveAll(typeof(ILogger<>));
                services.AddLogging(builder => builder.ClearProviders());

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                try
                {
                    // Ensure the database is created
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    // Log errors for debugging
                    var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory>>();
                    logger?.LogError(ex, "An error occurred seeding the database with test data");
                    throw;
                }
            });

            builder.UseEnvironment("Testing");
        }

        /// <summary>
        /// Gets a scoped service for testing
        /// </summary>
        public T GetService<T>() where T : class
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Gets the test database context
        /// </summary>
        public ApplicationDbContext GetDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        /// <summary>
        /// Cleans and reseeds the database
        /// </summary>
        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            // Note: Test data seeding would go here
        }
    }
}
