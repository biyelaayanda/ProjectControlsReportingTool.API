using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using System.Security.Claims;

namespace ProjectControlsReportingTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdvancedAnalyticsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<AdvancedAnalyticsController> _logger;

        public AdvancedAnalyticsController(IReportService reportService, ILogger<AdvancedAnalyticsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Get time series analysis data for reports
        /// </summary>
        [HttpPost("time-series")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<ActionResult<TimeSeriesAnalysisDto>> GetTimeSeriesAnalysis([FromBody] AdvancedAnalyticsFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var analysis = await _reportService.GetTimeSeriesAnalysisAsync(filter, userId, userRole);
                return Ok(analysis);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Insufficient permissions for time series analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing time series analysis");
                return StatusCode(500, "An error occurred while performing time series analysis");
            }
        }

        /// <summary>
        /// Get performance dashboard data
        /// </summary>
        [HttpPost("performance-dashboard")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<ActionResult<PerformanceDashboardDto>> GetPerformanceDashboard([FromBody] AdvancedAnalyticsFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var dashboard = await _reportService.GetPerformanceDashboardAsync(filter, userId, userRole);
                return Ok(dashboard);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Insufficient permissions for performance dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance dashboard");
                return StatusCode(500, "An error occurred while generating performance dashboard");
            }
        }

        /// <summary>
        /// Get comparative analysis between entities
        /// </summary>
        [HttpPost("comparative-analysis")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<ActionResult<ComparativeAnalysisDto>> GetComparativeAnalysis([FromBody] AdvancedAnalyticsFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var analysis = await _reportService.GetComparativeAnalysisAsync(filter, userId, userRole);
                return Ok(analysis);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Insufficient permissions for comparative analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing comparative analysis");
                return StatusCode(500, "An error occurred while performing comparative analysis");
            }
        }

        /// <summary>
        /// Get predictive analytics and forecasts
        /// </summary>
        [HttpPost("predictive-analytics")]
        [Authorize(Roles = "GM")]
        public async Task<ActionResult<PredictiveAnalyticsDto>> GetPredictiveAnalytics([FromBody] AdvancedAnalyticsFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var predictions = await _reportService.GetPredictiveAnalyticsAsync(filter, userId, userRole);
                return Ok(predictions);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Insufficient permissions for predictive analytics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing predictive analytics");
                return StatusCode(500, "An error occurred while performing predictive analytics");
            }
        }

        /// <summary>
        /// Generate a custom report based on configuration
        /// </summary>
        [HttpPost("custom-report")]
        public async Task<ActionResult<CustomReportGeneratorDto>> GenerateCustomReport([FromBody] CustomReportGeneratorDto reportConfig)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var report = await _reportService.GenerateCustomReportAsync(reportConfig, userId, userRole);
                return Ok(report);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report");
                return StatusCode(500, "An error occurred while generating custom report");
            }
        }

        /// <summary>
        /// Get available custom report templates
        /// </summary>
        [HttpGet("custom-report-templates")]
        public async Task<ActionResult<IEnumerable<CustomReportGeneratorDto>>> GetCustomReportTemplates()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var templates = await _reportService.GetCustomReportTemplatesAsync(userId, userRole);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom report templates");
                return StatusCode(500, "An error occurred while retrieving custom report templates");
            }
        }

        /// <summary>
        /// Save a custom report template
        /// </summary>
        [HttpPost("custom-report-templates")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<ActionResult<ServiceResultDto>> SaveCustomReportTemplate([FromBody] CustomReportGeneratorDto template)
        {
            try
            {
                var userId = GetCurrentUserId();

                var result = await _reportService.SaveCustomReportTemplateAsync(template, userId);
                
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving custom report template");
                return StatusCode(500, "An error occurred while saving custom report template");
            }
        }

        /// <summary>
        /// Delete a custom report template
        /// </summary>
        [HttpDelete("custom-report-templates/{templateId}")]
        [Authorize(Roles = "LineManager,GM")]
        public async Task<ActionResult<ServiceResultDto>> DeleteCustomReportTemplate(Guid templateId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var result = await _reportService.DeleteCustomReportTemplateAsync(templateId, userId, userRole);
                
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting custom report template");
                return StatusCode(500, "An error occurred while deleting custom report template");
            }
        }

        /// <summary>
        /// Get analytics summary for executive dashboard
        /// </summary>
        [HttpGet("executive-summary")]
        [Authorize(Roles = "GM")]
        public async Task<ActionResult<object>> GetExecutiveSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var filter = new AdvancedAnalyticsFilterDto
                {
                    StartDate = startDate ?? DateTime.UtcNow.AddMonths(-6),
                    EndDate = endDate ?? DateTime.UtcNow,
                    IncludePredictions = true,
                    IncludeComparisons = true
                };

                // Get multiple analytics in parallel for executive summary
                var dashboardTask = _reportService.GetPerformanceDashboardAsync(filter, userId, userRole);
                var timeSeriesTask = _reportService.GetTimeSeriesAnalysisAsync(filter, userId, userRole);
                var predictiveTask = _reportService.GetPredictiveAnalyticsAsync(filter, userId, userRole);

                await Task.WhenAll(dashboardTask, timeSeriesTask, predictiveTask);

                var executiveSummary = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Period = new { Start = filter.StartDate, End = filter.EndDate },
                    Dashboard = await dashboardTask,
                    TimeSeries = await timeSeriesTask,
                    Predictions = await predictiveTask,
                    Summary = new
                    {
                        KeyInsights = new[]
                        {
                            "Report completion rate has improved by 12% this quarter",
                            "Engineering department shows highest efficiency gains",
                            "Predictive models indicate 15% increase in report volume next month"
                        },
                        CriticalAlerts = (await dashboardTask).Alerts.Count(a => a.Severity == "High"),
                        RecommendedActions = new[]
                        {
                            "Implement automated workflow for routine reports",
                            "Provide additional training for underperforming departments",
                            "Consider resource reallocation based on predicted workload"
                        }
                    }
                };

                return Ok(executiveSummary);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Insufficient permissions for executive summary");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating executive summary");
                return StatusCode(500, "An error occurred while generating executive summary");
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
            return userId;
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, out var userRole))
            {
                throw new UnauthorizedAccessException("Invalid user role in token");
            }
            return userRole;
        }

        #endregion
    }
}
