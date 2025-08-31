using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    /// <summary>
    /// Controller for managing push notifications and subscriptions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PushNotificationsController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PushNotificationsController> _logger;

        public PushNotificationsController(
            IPushNotificationService pushNotificationService,
            ILogger<PushNotificationsController> logger)
        {
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        #region Subscription Management

        /// <summary>
        /// Creates a new push notification subscription for the current user
        /// </summary>
        /// <param name="dto">Subscription creation data</param>
        /// <returns>Created subscription details</returns>
        [HttpPost("subscriptions")]
        [ProducesResponseType(typeof(PushNotificationSubscriptionDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PushNotificationSubscriptionDto>> CreateSubscription([FromBody] CreatePushNotificationSubscriptionDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var subscription = await _pushNotificationService.CreateSubscriptionAsync(userId, dto);

                _logger.LogInformation("User {UserId} created push notification subscription {SubscriptionId}",
                    userId, subscription.Id);

                return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid subscription creation request: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating push notification subscription");
                return StatusCode(500, new { error = "An error occurred while creating the subscription" });
            }
        }

        /// <summary>
        /// Gets all push notification subscriptions for the current user
        /// </summary>
        /// <returns>List of user's subscriptions</returns>
        [HttpGet("subscriptions")]
        [ProducesResponseType(typeof(List<PushNotificationSubscriptionDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<PushNotificationSubscriptionDto>>> GetUserSubscriptions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var subscriptions = await _pushNotificationService.GetUserSubscriptionsAsync(userId);

                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user subscriptions");
                return StatusCode(500, new { error = "An error occurred while retrieving subscriptions" });
            }
        }

        /// <summary>
        /// Gets a specific push notification subscription by ID
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>Subscription details</returns>
        [HttpGet("subscriptions/{id:guid}")]
        [ProducesResponseType(typeof(PushNotificationSubscriptionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PushNotificationSubscriptionDto>> GetSubscription(Guid id)
        {
            try
            {
                var subscription = await _pushNotificationService.GetSubscriptionAsync(id);

                if (subscription == null)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only see their own subscriptions unless they are admin/manager
                if (subscription.UserId != currentUserId && 
                    currentUserRole != UserRole.GM && 
                    currentUserRole != UserRole.LineManager)
                {
                    return Forbid();
                }

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription {SubscriptionId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the subscription" });
            }
        }

        /// <summary>
        /// Updates a push notification subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="dto">Update data</param>
        /// <returns>Updated subscription details</returns>
        [HttpPut("subscriptions/{id:guid}")]
        [ProducesResponseType(typeof(PushNotificationSubscriptionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PushNotificationSubscriptionDto>> UpdateSubscription(Guid id, [FromBody] UpdatePushNotificationSubscriptionDto dto)
        {
            try
            {
                var existingSubscription = await _pushNotificationService.GetSubscriptionAsync(id);

                if (existingSubscription == null)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only update their own subscriptions unless they are admin/manager
                if (existingSubscription.UserId != currentUserId && 
                    currentUserRole != UserRole.GM && 
                    currentUserRole != UserRole.LineManager)
                {
                    return Forbid();
                }

                var updatedSubscription = await _pushNotificationService.UpdateSubscriptionAsync(id, dto);

                _logger.LogInformation("Updated push notification subscription {SubscriptionId}", id);

                return Ok(updatedSubscription);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid subscription update request for {SubscriptionId}: {Error}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription {SubscriptionId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the subscription" });
            }
        }

        /// <summary>
        /// Deletes a push notification subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("subscriptions/{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            try
            {
                var existingSubscription = await _pushNotificationService.GetSubscriptionAsync(id);

                if (existingSubscription == null)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Users can only delete their own subscriptions unless they are admin/manager
                if (existingSubscription.UserId != currentUserId && 
                    currentUserRole != UserRole.GM && 
                    currentUserRole != UserRole.LineManager)
                {
                    return Forbid();
                }

                var deleted = await _pushNotificationService.DeleteSubscriptionAsync(id);

                if (!deleted)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                _logger.LogInformation("Deleted push notification subscription {SubscriptionId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription {SubscriptionId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the subscription" });
            }
        }

        #endregion

        #region Push Notification Sending

        /// <summary>
        /// Sends a push notification to targeted users (admin/manager only)
        /// </summary>
        /// <param name="dto">Notification data and targeting criteria</param>
        /// <returns>Delivery results</returns>
        [HttpPost("send")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        [ProducesResponseType(typeof(PushNotificationDeliveryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PushNotificationDeliveryDto>> SendNotification([FromBody] SendPushNotificationDto dto)
        {
            try
            {
                var result = await _pushNotificationService.SendNotificationAsync(dto);

                _logger.LogInformation("Sent push notification '{Title}' to {TotalTargeted} subscriptions. Success: {SuccessfulDeliveries}, Failed: {FailedDeliveries}",
                    dto.Title, result.TotalTargeted, result.SuccessfulDeliveries, result.FailedDeliveries);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid notification send request: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification");
                return StatusCode(500, new { error = "An error occurred while sending the notification" });
            }
        }

        /// <summary>
        /// Sends a test push notification
        /// </summary>
        /// <param name="dto">Test notification data</param>
        /// <returns>Delivery results</returns>
        [HttpPost("test")]
        [ProducesResponseType(typeof(PushNotificationDeliveryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PushNotificationDeliveryDto>> SendTestNotification([FromBody] TestPushNotificationDto dto)
        {
            try
            {
                // If no target user specified, send to current user
                if (!dto.TargetUserId.HasValue)
                {
                    dto.TargetUserId = GetCurrentUserId();
                }
                else
                {
                    // Only admin/manager can send test notifications to other users
                    var currentUserRole = GetCurrentUserRole();
                    var currentUserId = GetCurrentUserId();

                    if (dto.TargetUserId != currentUserId && 
                        currentUserRole != UserRole.GM && 
                        currentUserRole != UserRole.LineManager)
                    {
                        return Forbid();
                    }
                }

                var result = await _pushNotificationService.SendTestNotificationAsync(dto);

                _logger.LogInformation("Sent test push notification to user {UserId}. Success: {SuccessfulDeliveries}, Failed: {FailedDeliveries}",
                    dto.TargetUserId, result.SuccessfulDeliveries, result.FailedDeliveries);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid test notification request: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test push notification");
                return StatusCode(500, new { error = "An error occurred while sending the test notification" });
            }
        }

        #endregion

        #region Search and Statistics

        /// <summary>
        /// Searches push notification subscriptions with filtering and pagination (admin/manager only)
        /// </summary>
        /// <param name="searchDto">Search and filter criteria</param>
        /// <returns>Paginated list of subscriptions</returns>
        [HttpPost("subscriptions/search")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> SearchSubscriptions([FromBody] PushNotificationSubscriptionSearchDto searchDto)
        {
            try
            {
                var (subscriptions, totalCount) = await _pushNotificationService.SearchSubscriptionsAsync(searchDto);

                var response = new
                {
                    subscriptions,
                    totalCount,
                    page = searchDto.Page,
                    pageSize = searchDto.PageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid subscription search request: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching push notification subscriptions");
                return StatusCode(500, new { error = "An error occurred while searching subscriptions" });
            }
        }

        /// <summary>
        /// Gets push notification subscription statistics (admin/manager only)
        /// </summary>
        /// <returns>Subscription statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        [ProducesResponseType(typeof(PushNotificationSubscriptionStatsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PushNotificationSubscriptionStatsDto>> GetStatistics()
        {
            try
            {
                var stats = await _pushNotificationService.GetSubscriptionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting push notification statistics");
                return StatusCode(500, new { error = "An error occurred while retrieving statistics" });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Processes bulk operations on multiple subscriptions (admin/manager only)
        /// </summary>
        /// <param name="dto">Bulk operation data</param>
        /// <returns>Bulk operation results</returns>
        [HttpPost("subscriptions/bulk")]
        [Authorize(Roles = nameof(UserRole.GM) + "," + nameof(UserRole.LineManager))]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> ProcessBulkOperation([FromBody] BulkPushNotificationOperationDto dto)
        {
            try
            {
                var result = await _pushNotificationService.ProcessBulkOperationAsync(dto);

                _logger.LogInformation("Processed bulk operation {Operation} on {TotalItems} subscriptions. Success: {SuccessfulItems}, Failed: {FailedItems}",
                    dto.Operation, result.TotalItems, result.SuccessfulItems, result.FailedItems);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid bulk operation request: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk operation");
                return StatusCode(500, new { error = "An error occurred while processing the bulk operation" });
            }
        }

        #endregion

        #region Health Check

        /// <summary>
        /// Checks the health of the push notification service
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult GetHealth()
        {
            return Ok(new 
            { 
                status = "healthy", 
                service = "PushNotificationService",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, out var role))
            {
                return UserRole.GeneralStaff;
            }
            return role;
        }

        #endregion
    }
}
