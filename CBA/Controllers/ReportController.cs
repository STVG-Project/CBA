using Microsoft.AspNetCore.Mvc;

namespace CBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("showPlotCount")]
        public IActionResult ListDevice([FromHeader] string token, string begin, string end)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                return Ok(Program.api_report.showPlotCount(begin, end));
            }
            else
            {
                return Unauthorized();
            }

        }
    }
}
