using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Tests.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ProjectControlsReportingTool.API.Tests.IntegrationTests
{
    /// <summary>
    /// Integration tests for Health Check endpoints
    /// Tests basic API connectivity and service health
    /// </summary>
    public class HealthCheckTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public HealthCheckTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_Get_ReturnsHealthy()
        {
            // Act
            var response = await _client.GetAsync("/api/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            
            // Verify the response contains health status
            Assert.Contains("Healthy", content);
        }

        [Fact]
        public async Task HealthCheck_Database_IsConnected()
        {
            // Arrange - Get the database context from the test factory
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act - Try to connect to the database
            var canConnect = await dbContext.Database.CanConnectAsync();

            // Assert
            Assert.True(canConnect, "Database should be accessible");
        }

        [Fact]
        public async Task HealthCheck_DatabaseHasTestData()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act
            var userCount = dbContext.Users.Count();
            var reportCount = dbContext.Reports.Count();

            // Assert
            Assert.True(userCount > 0, "Test database should contain users");
            Assert.True(reportCount > 0, "Test database should contain reports");
        }

        [Fact]
        public async Task Api_Endpoints_AreAccessible()
        {
            // Arrange - Test basic API endpoint accessibility
            var endpoints = new[]
            {
                "/api/health",
                "/swagger/index.html"
            };

            foreach (var endpoint in endpoints)
            {
                // Act
                var response = await _client.GetAsync(endpoint);

                // Assert
                Assert.True(
                    response.StatusCode == HttpStatusCode.OK || 
                    response.StatusCode == HttpStatusCode.Unauthorized, // Expected for protected endpoints
                    $"Endpoint {endpoint} should be accessible. Got: {response.StatusCode}");
            }
        }

        [Fact]
        public async Task Api_InvalidEndpoint_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/nonexistent");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Api_OptionsRequest_ReturnsCorsPreflight()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/health");
            request.Headers.Add("Origin", "http://localhost:3000");
            request.Headers.Add("Access-Control-Request-Method", "GET");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.NoContent,
                "CORS preflight should be handled");
        }
    }
}
