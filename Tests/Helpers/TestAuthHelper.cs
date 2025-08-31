using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Tests.Helpers
{
    /// <summary>
    /// Helper for creating JWT tokens for testing authentication
    /// </summary>
    public static class TestAuthHelper
    {
        private const string TestSecretKey = "this-is-a-test-secret-key-for-jwt-token-generation-in-testing-environment-only";
        private const string TestIssuer = "test-issuer";
        private const string TestAudience = "test-audience";

        /// <summary>
        /// Creates a JWT token for testing with specified user details
        /// </summary>
        public static string CreateTestToken(
            Guid userId,
            string username = "testuser",
            UserRole role = UserRole.GeneralStaff,
            Department department = Department.ProjectSupport,
            int expirationMinutes = 60)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(TestSecretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, role.ToString()),
                new("Department", department.ToString()),
                new("UserId", userId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = TestIssuer,
                Audience = TestAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Creates a token for a General Staff user
        /// </summary>
        public static string CreateGeneralStaffToken(Guid? userId = null)
        {
            var testUserId = userId ?? Guid.Parse("11111111-1111-1111-1111-111111111111");
            return CreateTestToken(testUserId, "staff", UserRole.GeneralStaff, Department.ProjectSupport);
        }

        /// <summary>
        /// Creates a token for a Line Manager user
        /// </summary>
        public static string CreateLineManagerToken(Guid? userId = null)
        {
            var testUserId = userId ?? Guid.Parse("22222222-2222-2222-2222-222222222222");
            return CreateTestToken(testUserId, "manager", UserRole.LineManager, Department.DocManagement);
        }

        /// <summary>
        /// Creates a token for a GM user
        /// </summary>
        public static string CreateGMToken(Guid? userId = null)
        {
            var testUserId = userId ?? Guid.Parse("33333333-3333-3333-3333-333333333333");
            return CreateTestToken(testUserId, "gm", UserRole.GM, Department.QS);
        }

        /// <summary>
        /// Creates an expired token for testing token expiration
        /// </summary>
        public static string CreateExpiredToken(Guid? userId = null)
        {
            var testUserId = userId ?? Guid.Parse("99999999-9999-9999-9999-999999999999");
            return CreateTestToken(testUserId, "expired", UserRole.GeneralStaff, Department.ProjectSupport, -60); // Expired 1 hour ago
        }

        /// <summary>
        /// Gets the test secret key for JWT configuration in tests
        /// </summary>
        public static string GetTestSecretKey() => TestSecretKey;

        /// <summary>
        /// Gets test JWT issuer
        /// </summary>
        public static string GetTestIssuer() => TestIssuer;

        /// <summary>
        /// Gets test JWT audience
        /// </summary>
        public static string GetTestAudience() => TestAudience;

        /// <summary>
        /// Creates authorization header value for HTTP requests
        /// </summary>
        public static string CreateAuthorizationHeader(string token)
        {
            return $"Bearer {token}";
        }

        /// <summary>
        /// Creates a ClaimsPrincipal for testing controller actions directly
        /// </summary>
        public static ClaimsPrincipal CreateTestPrincipal(
            Guid userId,
            string username = "testuser",
            UserRole role = UserRole.GeneralStaff,
            Department department = Department.ProjectSupport)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, role.ToString()),
                new("Department", department.ToString()),
                new("UserId", userId.ToString())
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }
    }
}
