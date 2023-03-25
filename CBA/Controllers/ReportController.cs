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
        public IActionResult showPlotCount(string time)
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

        [HttpGet]
        [Route("showPlotCountForDate")]
        public IActionResult showPlotCountForDate(string begin, string end)
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

            return Ok(Program.api_report.getCountDates(time_begin, time_end));
        }


        [HttpGet]
        [Route("showPlotPerson")]
        public IActionResult showPlotPerson(string time)
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

            return Ok(Program.api_report.getPersonsHours(time_input));

        }

        [HttpGet]
        [Route("showPlotCountPersonsForDate")]
        public IActionResult showPlotCountPersonsForDate(string begin, string end)
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

            return Ok(Program.api_report.getCountPersonForDate(time_begin, time_end));
        }

        [HttpGet]
        [Route("showCountWithDevice")]
        public IActionResult showCountWithDevice(string time)
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

            return Ok(Program.api_report.getCountWithDevice(time_input));

        }

        [HttpGet]
        [Route("showPlotWithDeviceForDate")]
        public IActionResult showPlotWithDeviceForDate(string begin, string end)
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

            return Ok(Program.api_report.getCountWithDeviceForDates(time_begin, time_end));
        }
    }
}
