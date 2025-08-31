using System.Collections.Concurrent;
using System.Net;

namespace ProjectControlsReportingTool.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _maxRequests = 100; // Max requests per time window
            _timeWindow = TimeSpan.FromMinutes(1); // 1 minute window
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var clientInfo = _clients.GetOrAdd(clientId, new ClientRequestInfo());

            lock (clientInfo)
            {
                var now = DateTime.UtcNow;
                
                // Clean old requests outside the time window
                clientInfo.Requests.RemoveAll(time => now - time > _timeWindow);
                
                // Check if client has exceeded rate limit
                if (clientInfo.Requests.Count >= _maxRequests)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString();
                    
                    _logger.LogWarning(
                        "Rate limit exceeded for client {ClientId} from IP {IP}",
                        clientId,
                        context.Connection.RemoteIpAddress?.ToString()
                    );
                    
                    return;
                }
                
                // Add current request timestamp
                clientInfo.Requests.Add(now);
            }

            await _next(context);
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            // Use IP address as primary identifier
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // For authenticated users, also include user ID for more accurate limiting
            var userId = context.User?.Identity?.Name;
            
            return string.IsNullOrEmpty(userId) ? ipAddress : $"{ipAddress}_{userId}";
        }

        private class ClientRequestInfo
        {
            public List<DateTime> Requests { get; } = new();
        }
    }
}
