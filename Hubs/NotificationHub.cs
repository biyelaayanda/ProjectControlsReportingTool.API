using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications and updates
    /// Provides instant communication for report status changes, workflow updates, and system alerts
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private static readonly Dictionary<string, string> _userConnections = new();

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        #region Connection Management

        /// <summary>
        /// Called when a client connects to the hub
        /// Manages user-connection mapping for targeted notifications
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Map user to connection for targeted messaging
                    _userConnections[userId] = Context.ConnectionId;
                    
                    // Add to authenticated users group
                    await Groups.AddToGroupAsync(Context.ConnectionId, "AuthenticatedUsers");
                    
                    // Join role-based groups
                    var userRoles = Context.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value);
                    if (userRoles != null)
                    {
                        foreach (var role in userRoles)
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
                        }
                    }

                    _logger.LogInformation("User {UserId} ({UserEmail}) connected with ConnectionId: {ConnectionId}", 
                        userId, userEmail, Context.ConnectionId);

                    // Notify user of successful connection
                    await Clients.Caller.SendAsync("ConnectionEstablished", new
                    {
                        ConnectionId = Context.ConnectionId,
                        ConnectedAt = DateTime.UtcNow,
                        Message = "Real-time notifications enabled"
                    });
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for ConnectionId: {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// Cleans up user-connection mappings
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!string.IsNullOrEmpty(userId) && _userConnections.ContainsKey(userId))
                {
                    _userConnections.Remove(userId);
                    _logger.LogInformation("User {UserId} disconnected. ConnectionId: {ConnectionId}", 
                        userId, Context.ConnectionId);
                }

                if (exception != null)
                {
                    _logger.LogWarning(exception, "User disconnected with exception. ConnectionId: {ConnectionId}", 
                        Context.ConnectionId);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for ConnectionId: {ConnectionId}", Context.ConnectionId);
            }
        }

        #endregion

        #region Client Methods

        /// <summary>
        /// Allows clients to join specific notification groups
        /// </summary>
        /// <param name="groupName">Name of the group to join (e.g., "Project_123", "Department_Engineering")</param>
        public async Task JoinNotificationGroup(string groupName)
        {
            try
            {
                if (string.IsNullOrEmpty(groupName))
                {
                    await Clients.Caller.SendAsync("Error", "Group name cannot be empty");
                    return;
                }

                // Validate group name format and user permissions
                if (IsValidGroupForUser(groupName))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    
                    _logger.LogInformation("User {UserId} joined group: {GroupName}", 
                        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, groupName);
                    
                    await Clients.Caller.SendAsync("GroupJoined", new
                    {
                        GroupName = groupName,
                        JoinedAt = DateTime.UtcNow,
                        Message = $"Successfully joined {groupName} notifications"
                    });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Access denied to group: " + groupName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining group {GroupName} for user {UserId}", 
                    groupName, Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await Clients.Caller.SendAsync("Error", "Failed to join group");
            }
        }

        /// <summary>
        /// Allows clients to leave specific notification groups
        /// </summary>
        /// <param name="groupName">Name of the group to leave</param>
        public async Task LeaveNotificationGroup(string groupName)
        {
            try
            {
                if (string.IsNullOrEmpty(groupName))
                {
                    await Clients.Caller.SendAsync("Error", "Group name cannot be empty");
                    return;
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                
                _logger.LogInformation("User {UserId} left group: {GroupName}", 
                    Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, groupName);
                
                await Clients.Caller.SendAsync("GroupLeft", new
                {
                    GroupName = groupName,
                    LeftAt = DateTime.UtcNow,
                    Message = $"Left {groupName} notifications"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group {GroupName} for user {UserId}", 
                    groupName, Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await Clients.Caller.SendAsync("Error", "Failed to leave group");
            }
        }

        /// <summary>
        /// Handles client ping for connection health monitoring
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        #endregion

        #region Server Methods (for other services to call)

        /// <summary>
        /// Send notification to a specific user
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="notification">Notification data</param>
        public async Task SendToUser(string userId, object notification)
        {
            try
            {
                if (_userConnections.TryGetValue(userId, out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("Notification", notification);
                    _logger.LogDebug("Notification sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogDebug("User {UserId} not connected for real-time notification", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Send notification to a specific group
        /// </summary>
        /// <param name="groupName">Target group name</param>
        /// <param name="notification">Notification data</param>
        public async Task SendToGroup(string groupName, object notification)
        {
            try
            {
                await Clients.Group(groupName).SendAsync("Notification", notification);
                _logger.LogDebug("Notification sent to group {GroupName}", groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to group {GroupName}", groupName);
            }
        }

        /// <summary>
        /// Broadcast notification to all connected users
        /// </summary>
        /// <param name="notification">Notification data</param>
        public async Task BroadcastToAll(object notification)
        {
            try
            {
                await Clients.All.SendAsync("Notification", notification);
                _logger.LogDebug("Notification broadcasted to all users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notification to all users");
            }
        }

        /// <summary>
        /// Send report status update to relevant users
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="status">New status</param>
        /// <param name="message">Update message</param>
        public async Task SendReportStatusUpdate(int reportId, string status, string message)
        {
            try
            {
                var update = new
                {
                    Type = "ReportStatusUpdate",
                    ReportId = reportId,
                    Status = status,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                // Send to report-specific group
                await Clients.Group($"Report_{reportId}").SendAsync("ReportStatusUpdate", update);
                
                // Send to all authenticated users for dashboard updates
                await Clients.Group("AuthenticatedUsers").SendAsync("DashboardUpdate", update);
                
                _logger.LogInformation("Report status update sent for Report {ReportId}: {Status}", reportId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending report status update for Report {ReportId}", reportId);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates if the current user can join the specified group
        /// </summary>
        /// <param name="groupName">Group name to validate</param>
        /// <returns>True if user can join the group</returns>
        private bool IsValidGroupForUser(string groupName)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = Context.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList() ?? new List<string>();

                // Allow standard groups
                if (groupName == "AuthenticatedUsers")
                    return true;

                // Allow role-based groups if user has the role
                if (groupName.StartsWith("Role_"))
                {
                    var requiredRole = groupName.Substring(5);
                    return userRoles.Contains(requiredRole);
                }

                // Allow project-specific groups (implement project access validation)
                if (groupName.StartsWith("Project_"))
                {
                    // TODO: Validate user has access to the specific project
                    return true;
                }

                // Allow department groups (implement department validation)
                if (groupName.StartsWith("Department_"))
                {
                    // TODO: Validate user belongs to the department
                    return true;
                }

                // Allow report-specific groups (implement report access validation)
                if (groupName.StartsWith("Report_"))
                {
                    // TODO: Validate user has access to the specific report
                    return true;
                }

                // Deny unknown group patterns
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating group access for {GroupName}", groupName);
                return false;
            }
        }

        /// <summary>
        /// Gets the current connection statistics
        /// </summary>
        /// <returns>Connection statistics</returns>
        public static object GetConnectionStats()
        {
            return new
            {
                ConnectedUsers = _userConnections.Count,
                ActiveConnections = _userConnections.Values.Count,
                LastUpdated = DateTime.UtcNow
            };
        }

        #endregion
    }
}
