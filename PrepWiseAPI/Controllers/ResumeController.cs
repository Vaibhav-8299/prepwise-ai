using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrepWiseAPI.Services;

namespace PrepWiseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResumeController : ControllerBase
    {
        private readonly ResumeService _resumeService;

        public ResumeController(ResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // POST /api/resume/upload-and-analyze
        [HttpPost("upload-and-analyze")]
        public async Task<IActionResult> UploadAndAnalyze([FromForm] IFormFile file, [FromForm] string targetRole)
        {
            try
            {
                var userId = GetUserId();
                var report = await _resumeService.UploadAndAnalyzeAsync(userId, file, targetRole);
                return Ok(new { message = "Resume analyzed successfully!", data = report });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/resume/reports
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var userId = GetUserId();
            var reports = await _resumeService.GetUserReportsAsync(userId);
            return Ok(new { data = reports });
        }

        // GET /api/resume/report/{id}
        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var userId = GetUserId();
            var report = await _resumeService.GetReportByIdAsync(userId, id);

            if (report == null)
                return NotFound(new { message = "Report not found." });

            return Ok(new { data = report });
        }
    }
}
