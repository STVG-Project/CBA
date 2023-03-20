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
        public IActionResult showPlotCount([FromHeader] string token, string time)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                DateTime time_input = DateTime.MinValue;
                try
                {
                    time_input = DateTime.ParseExact(time, "dd-MM-yyyy", null);
                }
                catch (Exception e)
                {
                    time_input = DateTime.MinValue;
                }

                
                return Ok(Program.api_report.getCountHour(time_input));
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpGet]
        [Route("showPlotCountForDate")]
        public IActionResult showPlotCountForDate([FromHeader] string token, string begin, string end)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                DateTime time_begin = DateTime.MinValue;
                try
                {
                    time_begin = DateTime.ParseExact(begin, "dd-MM-yyyy", null);
                }
                catch (Exception e)
                {
                    time_begin = DateTime.MinValue;
                }

                DateTime time_end = DateTime.MaxValue;
                try
                {
                    time_end = DateTime.ParseExact(end, "dd-MM-yyyy", null);
                }
                catch (Exception e)
                {
                    time_end = DateTime.MaxValue;
                }


                return Ok(Program.api_report.getCountDate(time_begin, time_end));
            }
            else
            {
                return Unauthorized();
            }

        }
    }
}
