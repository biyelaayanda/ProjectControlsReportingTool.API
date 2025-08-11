using Microsoft.AspNetCore.Mvc;

namespace ProjectControlsReportingTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new 
            { 
                Status = "Healthy",
                Message = "Project Controls Reporting Tool API is running",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        [HttpGet("database")]
        public IActionResult CheckDatabase()
        {
            try
            {
                // This will be implemented once we have repositories set up
                return Ok(new 
                { 
                    Status = "Connected",
                    Message = "Database connection successful",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    Status = "Error",
                    Message = "Database connection failed",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
