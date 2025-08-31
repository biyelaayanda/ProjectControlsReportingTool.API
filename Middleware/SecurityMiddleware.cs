using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Middleware
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;

        public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            AddSecurityHeaders(context);

            // Log request for audit
            LogRequest(context);

            await _next(context);
        }

        private static void AddSecurityHeaders(HttpContext context)
        {
            var response = context.Response;

            // Prevent clickjacking
            response.Headers["X-Frame-Options"] = "DENY";

            // Prevent MIME-type sniffing
            response.Headers["X-Content-Type-Options"] = "nosniff";

            // XSS protection
            response.Headers["X-XSS-Protection"] = "1; mode=block";

            // Referrer policy
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Enhanced Content Security Policy (more restrictive)
            response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self';";

            // HSTS (only for HTTPS)
            if (context.Request.IsHttps)
            {
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }
        }

        private void LogRequest(HttpContext context)
        {
            var request = context.Request;
            var userAgent = request.Headers.UserAgent.ToString();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation(
                "Security Audit: {Method} {Path} from IP {IP} User-Agent {UserAgent} User {UserId}",
                request.Method,
                request.Path,
                ipAddress,
                userAgent,
                userId ?? "Anonymous"
            );
        }
    }
}
