using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for managing user notification preferences
    /// Provides RESTful endpoints for notification preference management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationPreferencesController : ControllerBase
    {
        private readonly IUserNotificationPreferenceService _preferenceService;
        private readonly ILogger<NotificationPreferencesController> _logger;

        public NotificationPreferencesController(
            IUserNotificationPreferenceService preferenceService,
            ILogger<NotificationPreferencesController> logger)
        {
            _preferenceService = preferenceService;
            _logger = logger;
        }

        #region User Preference Management

        /// <summary>
        /// Get all notification preferences for the current user
        /// </summary>
        /// <returns>List of user notification preferences</returns>
        [HttpGet]
        public async Task<ActionResult<List<UserNotificationPreferenceDto>>> GetUserPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var preferences = await _preferenceService.GetUserPreferencesAsync(userId.Value);
                
                _logger.LogInformation("Retrieved {Count} notification preferences for user {UserId}", 
                    preferences.Count, userId);
                
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences for current user");
                return StatusCode(500, "An error occurred while retrieving notification preferences");
            }
        }

        /// <summary>
        /// Get a specific notification preference for the current user
        /// </summary>
        /// <param name="notificationType">Type of notification</param>
        /// <returns>Notification preference or 404 if not found</returns>
        [HttpGet("{notificationType}")]
        public async Task<ActionResult<UserNotificationPreferenceDto>> GetUserPreference(string notificationType)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var preference = await _preferenceService.GetUserPreferenceAsync(userId.Value, notificationType);
                
                if (preference == null)
                {
                    _logger.LogWarning("Notification preference not found for user {UserId}, type {Type}", 
                        userId, notificationType);
                    return NotFound($"Notification preference for type '{notificationType}' not found");
                }

                _logger.LogInformation("Retrieved notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);
                
                return Ok(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preference for user, type {Type}", notificationType);
                return StatusCode(500, "An error occurred while retrieving the notification preference");
            }
        }

        /// <summary>
        /// Create or update a notification preference for the current user
        /// </summary>
        /// <param name="createDto">Preference creation data</param>
        /// <returns>Created/updated preference</returns>
        [HttpPost]
        public async Task<ActionResult<UserNotificationPreferenceDto>> SetUserPreference([FromBody] CreateUserNotificationPreferenceDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var preference = await _preferenceService.SetUserPreferenceAsync(userId.Value, createDto);
                
                _logger.LogInformation("Set notification preference for user {UserId}, type {Type}", 
                    userId, createDto.NotificationType);
                
                return CreatedAtAction(nameof(GetUserPreference), 
                    new { notificationType = preference.NotificationType }, preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting notification preference for user, type {Type}", 
                    createDto?.NotificationType);
                return StatusCode(500, "An error occurred while setting the notification preference");
            }
        }

        /// <summary>
        /// Update an existing notification preference for the current user
        /// </summary>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="updateDto">Preference update data</param>
        /// <returns>Updated preference</returns>
        [HttpPut("{notificationType}")]
        public async Task<ActionResult<UserNotificationPreferenceDto>> UpdateUserPreference(
            string notificationType, 
            [FromBody] UpdateUserNotificationPreferenceDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var preference = await _preferenceService.UpdateUserPreferenceAsync(userId.Value, notificationType, updateDto);
                
                _logger.LogInformation("Updated notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);
                
                return Ok(preference);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Notification preference not found for update: user {UserId}, type {Type}", 
                    GetCurrentUserId(), notificationType);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preference for user, type {Type}", notificationType);
                return StatusCode(500, "An error occurred while updating the notification preference");
            }
        }

        /// <summary>
        /// Delete a notification preference for the current user
        /// </summary>
        /// <param name="notificationType">Type of notification</param>
        /// <returns>Success status</returns>
        [HttpDelete("{notificationType}")]
        public async Task<ActionResult> DeleteUserPreference(string notificationType)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var success = await _preferenceService.DeleteUserPreferenceAsync(userId.Value, notificationType);
                
                if (!success)
                {
                    _logger.LogWarning("Notification preference not found for deletion: user {UserId}, type {Type}", 
                        userId, notificationType);
                    return NotFound($"Notification preference for type '{notificationType}' not found");
                }

                _logger.LogInformation("Deleted notification preference for user {UserId}, type {Type}", 
                    userId, notificationType);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification preference for user, type {Type}", notificationType);
                return StatusCode(500, "An error occurred while deleting the notification preference");
            }
        }

        /// <summary>
        /// Bulk update multiple notification preferences for the current user
        /// </summary>
        /// <param name="bulkUpdateDto">Bulk update data</param>
        /// <returns>Number of updated preferences</returns>
        [HttpPost("bulk-update")]
        public async Task<ActionResult<int>> BulkUpdatePreferences([FromBody] BulkNotificationPreferenceUpdateDto bulkUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var updatedCount = await _preferenceService.BulkUpdatePreferencesAsync(userId.Value, bulkUpdateDto);
                
                _logger.LogInformation("Bulk updated {Count} notification preferences for user {UserId}", 
                    updatedCount, userId);
                
                return Ok(updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating notification preferences for user");
                return StatusCode(500, "An error occurred while bulk updating notification preferences");
            }
        }

        #endregion

        #region Default Preferences

        /// <summary>
        /// Initialize default notification preferences for the current user
        /// </summary>
        /// <returns>Number of default preferences created</returns>
        [HttpPost("initialize-defaults")]
        public async Task<ActionResult<int>> InitializeDefaultPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var createdCount = await _preferenceService.InitializeDefaultPreferencesAsync(userId.Value);
                
                _logger.LogInformation("Initialized {Count} default notification preferences for user {UserId}", 
                    createdCount, userId);
                
                return Ok(createdCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default notification preferences for user");
                return StatusCode(500, "An error occurred while initializing default preferences");
            }
        }

        /// <summary>
        /// Reset user preferences to system defaults
        /// </summary>
        /// <returns>Number of preferences reset</returns>
        [HttpPost("reset-to-defaults")]
        public async Task<ActionResult<int>> ResetToDefaults()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var resetCount = await _preferenceService.ResetToDefaultsAsync(userId.Value);
                
                _logger.LogInformation("Reset {Count} notification preferences to defaults for user {UserId}", 
                    resetCount, userId);
                
                return Ok(resetCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting notification preferences to defaults for user");
                return StatusCode(500, "An error occurred while resetting preferences to defaults");
            }
        }

        /// <summary>
        /// Get available notification types for preference management
        /// </summary>
        /// <returns>List of notification types with descriptions</returns>
        [HttpGet("notification-types")]
        public async Task<ActionResult<List<NotificationTypeInfoDto>>> GetAvailableNotificationTypes()
        {
            try
            {
                var notificationTypes = await _preferenceService.GetAvailableNotificationTypesAsync();
                
                _logger.LogDebug("Retrieved {Count} available notification types", notificationTypes.Count);
                
                return Ok(notificationTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available notification types");
                return StatusCode(500, "An error occurred while retrieving notification types");
            }
        }

        #endregion

        #region Delivery Logic

        /// <summary>
        /// Get comprehensive delivery summary for a notification
        /// </summary>
        /// <param name="notificationType">Type of notification</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>Delivery summary with all channels</returns>
        [HttpGet("delivery-summary")]
        public async Task<ActionResult<NotificationDeliverySummaryDto>> GetDeliverySummary(
            [FromQuery] string notificationType, 
            [FromQuery] string priority)
        {
            try
            {
                if (string.IsNullOrEmpty(notificationType) || string.IsNullOrEmpty(priority))
                {
                    return BadRequest("NotificationType and Priority are required parameters");
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var deliverySummary = await _preferenceService.GetDeliverySummaryAsync(userId.Value, notificationType, priority);
                
                _logger.LogDebug("Generated delivery summary for user {UserId}, type {Type}, priority {Priority}", 
                    userId, notificationType, priority);
                
                return Ok(deliverySummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery summary for type {Type}, priority {Priority}", 
                    notificationType, priority);
                return StatusCode(500, "An error occurred while getting delivery summary");
            }
        }

        #endregion

        #region Quiet Hours

        /// <summary>
        /// Check if current time falls within user's quiet hours
        /// </summary>
        /// <returns>True if in quiet hours</returns>
        [HttpGet("quiet-hours/status")]
        public async Task<ActionResult<bool>> IsInQuietHours()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var isInQuietHours = await _preferenceService.IsInQuietHoursAsync(userId.Value);
                
                _logger.LogDebug("Quiet hours status for user {UserId}: {Status}", userId, isInQuietHours);
                
                return Ok(isInQuietHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiet hours status for user");
                return StatusCode(500, "An error occurred while checking quiet hours status");
            }
        }

        /// <summary>
        /// Update user's quiet hours settings for all preferences
        /// </summary>
        /// <param name="request">Quiet hours update request</param>
        /// <returns>Number of preferences updated</returns>
        [HttpPost("quiet-hours")]
        public async Task<ActionResult<int>> UpdateQuietHours([FromBody] UpdateQuietHoursRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var updatedCount = await _preferenceService.UpdateQuietHoursAsync(
                    userId.Value, 
                    request.StartTime, 
                    request.EndTime, 
                    request.TimeZone);
                
                _logger.LogInformation("Updated quiet hours for {Count} preferences for user {UserId}", 
                    updatedCount, userId);
                
                return Ok(updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiet hours for user");
                return StatusCode(500, "An error occurred while updating quiet hours");
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get notification preference statistics for the current user
        /// </summary>
        /// <returns>Preference statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<NotificationPreferenceStatsDto>> GetUserPreferenceStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                var stats = await _preferenceService.GetUserPreferenceStatsAsync(userId.Value);
                
                _logger.LogDebug("Retrieved preference statistics for user {UserId}", userId);
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preference statistics for user");
                return StatusCode(500, "An error occurred while retrieving preference statistics");
            }
        }

        /// <summary>
        /// Get system-wide notification preference statistics (Admin only)
        /// </summary>
        /// <returns>System preference statistics</returns>
        [HttpGet("statistics/system")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NotificationPreferenceStatsDto>> GetSystemPreferenceStats()
        {
            try
            {
                var stats = await _preferenceService.GetSystemPreferenceStatsAsync();
                
                _logger.LogInformation("Retrieved system preference statistics");
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system preference statistics");
                return StatusCode(500, "An error occurred while retrieving system statistics");
            }
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Get notification preferences for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user notification preferences</returns>
        [HttpGet("admin/users/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserNotificationPreferenceDto>>> GetUserPreferencesById(Guid userId)
        {
            try
            {
                var preferences = await _preferenceService.GetUserPreferencesAsync(userId);
                
                _logger.LogInformation("Admin retrieved {Count} notification preferences for user {UserId}", 
                    preferences.Count, userId);
                
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving notification preferences");
            }
        }

        /// <summary>
        /// Initialize default preferences for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of default preferences created</returns>
        [HttpPost("admin/users/{userId:guid}/initialize-defaults")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> InitializeDefaultPreferencesForUser(Guid userId)
        {
            try
            {
                var createdCount = await _preferenceService.InitializeDefaultPreferencesAsync(userId);
                
                _logger.LogInformation("Admin initialized {Count} default notification preferences for user {UserId}", 
                    createdCount, userId);
                
                return Ok(createdCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default notification preferences for user {UserId}", userId);
                return StatusCode(500, "An error occurred while initializing default preferences");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the current user ID from the JWT token
        /// </summary>
        /// <returns>User ID or null if not found</returns>
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token claims");
                return null;
            }

            return userId;
        }

        #endregion
    }

    #region Request DTOs

    /// <summary>
    /// Request DTO for updating quiet hours
    /// </summary>
    public class UpdateQuietHoursRequest
    {
        /// <summary>
        /// Quiet hours start time in HH:mm format (24-hour)
        /// </summary>
        public string? StartTime { get; set; }

        /// <summary>
        /// Quiet hours end time in HH:mm format (24-hour)
        /// </summary>
        public string? EndTime { get; set; }

        /// <summary>
        /// User's time zone identifier
        /// </summary>
        public string TimeZone { get; set; } = "UTC";
    }

    #endregion
}
