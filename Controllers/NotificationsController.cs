using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Phase 8: Basic Notifications Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            ApplicationDbContext context,
            ILogger<NotificationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get notifications for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ServiceResultDto>> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] NotificationStatus? status = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var query = _context.Notifications.Where(n => n.RecipientId == userId);
                
                if (status.HasValue)
                {
                    query = query.Where(n => n.Status == status.Value);
                }

                var notifications = await query
                    .OrderByDescending(n => n.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new
                    {
                        n.Id,
                        n.Title,
                        n.Message,
                        n.Type,
                        n.Priority,
                        n.Status,
                        n.Category,
                        n.CreatedDate,
                        n.ReadDate
                    })
                    .ToListAsync();

                return ServiceResultDto.SuccessResult(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return ServiceResultDto.ErrorResult("Failed to get notifications");
            }
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ServiceResultDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            try
            {
                var notification = new Notification
                {
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type,
                    Priority = dto.Priority,
                    RecipientId = dto.RecipientId,
                    SenderId = GetCurrentUserId(),
                    Category = dto.Category,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return ServiceResultDto.SuccessResult(new { notification.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return ServiceResultDto.ErrorResult("Failed to create notification");
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<ActionResult<ServiceResultDto>> MarkAsRead(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.RecipientId == userId);

                if (notification == null)
                {
                    return ServiceResultDto.ErrorResult("Notification not found");
                }

                notification.Status = NotificationStatus.Read;
                notification.ReadDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ServiceResultDto.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return ServiceResultDto.ErrorResult("Failed to mark notification as read");
            }
        }

        /// <summary>
        /// Get notification statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ServiceResultDto>> GetStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var stats = await _context.Notifications
                    .Where(n => n.RecipientId == userId)
                    .GroupBy(n => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Unread = g.Count(n => n.Status == NotificationStatus.Pending),
                        Read = g.Count(n => n.Status == NotificationStatus.Read)
                    })
                    .FirstOrDefaultAsync();

                return ServiceResultDto.SuccessResult(stats ?? new { Total = 0, Unread = 0, Read = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats");
                return ServiceResultDto.ErrorResult("Failed to get notification statistics");
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResultDto>> DeleteNotification(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.RecipientId == userId);

                if (notification == null)
                {
                    return ServiceResultDto.ErrorResult("Notification not found");
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return ServiceResultDto.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return ServiceResultDto.ErrorResult("Failed to delete notification");
            }
        }

        /// <summary>
        /// Send a system notification to all users (Admin only)
        /// </summary>
        [HttpPost("broadcast")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ServiceResultDto>> BroadcastNotification([FromBody] BroadcastNotificationDto dto)
        {
            try
            {
                var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
                var senderId = GetCurrentUserId();

                var notifications = users.Select(user => new Notification
                {
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = NotificationType.SystemAlert,
                    Priority = dto.Priority,
                    RecipientId = user.Id,
                    SenderId = senderId,
                    Category = "System",
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                return ServiceResultDto.SuccessResult(new { SentCount = notifications.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notification");
                return ServiceResultDto.ErrorResult("Failed to broadcast notification");
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>
    /// DTO for broadcasting system notifications
    /// </summary>
    public class BroadcastNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    }
}
