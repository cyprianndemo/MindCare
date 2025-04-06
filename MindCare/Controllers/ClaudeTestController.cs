using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MindCare.Services;

namespace MindCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaudeTestController : ControllerBase
    {
        private readonly ClaudeAiService _claudeService;

        public ClaudeTestController(ClaudeAiService claudeService)
        {
            _claudeService = claudeService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestClaudeConnection()
        {
            try
            {
                var response = await _claudeService.GetResponseAsync("Hello, can you verify that this connection is working?", "test-user");
                return Ok(new { success = true, message = response });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}