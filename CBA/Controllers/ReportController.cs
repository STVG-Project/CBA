using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using static CBA.APIs.MyReport;

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
        [Route("getStatisticsCountPersonForYear")]
        public IActionResult countPersonGroupForYear(string time, string group)
        {
            if (group.CompareTo("0") == 0)
            {
                group = "";
            }
            DateTime _time = DateTime.MinValue;
            try
            {
                _time = DateTime.ParseExact(time, "yyyy", null);
            }
            catch (Exception e)
            {
                _time = DateTime.MinValue;
            }

            return Ok(Program.api_report.getStatisticsCountPersonForYear(_time, group));
        }

        [HttpGet]
        [Route("getStatisticsCountPersonForMonth")]
        public IActionResult countPersonGroupForMonth(string time, string group)
        {
            if (group.CompareTo("0") == 0)
            {
                group = "";
            }
            DateTime _time = DateTime.MinValue;
            try
            {
                _time = DateTime.ParseExact(time, "MM-yyyy", null);
            }
            catch (Exception e)
            {
                _time = DateTime.MinValue;
            }

            return Ok(Program.api_report.getStatisticsCountPersonForMonth(_time, group));
        }

        [HttpGet]
        [Route("getDetailStatisticsCountPersonForYear")]
        public IActionResult countDetailPersonGroupForYear(string time)
        {
            DateTime _time = DateTime.MinValue;
            try
            {
                _time = DateTime.ParseExact(time, "yyyy", null);
            }
            catch (Exception e)
            {
                _time = DateTime.MinValue;
            }

            return Ok(Program.api_report.getDetailStatisticsForYear(_time));
        }

        [HttpGet]
        [Route("getComparePersonMOM")]
        public IActionResult getComparePersonMOM(string begin, string end)
        {
           
            DateTime m_begin = DateTime.MinValue;
            try
            {
                m_begin = DateTime.ParseExact(begin, "MM-yyyy", null);
            }
            catch (Exception e)
            {
                m_begin = DateTime.MinValue;
            }

            DateTime m_end = DateTime.MinValue;
            try
            {
                m_end = DateTime.ParseExact(end, "MM-yyyy", null);
            }
            catch (Exception e)
            {
                m_end = DateTime.MinValue;
            }

            return Ok(Program.api_report.getComparePersonMOM(m_begin, m_end));
        }
        [HttpGet]
        [Route("getStatisticsPersonForDevice")]
        public IActionResult countPersonForDeviceDateV2(string time)
        {
            DateTime _time = DateTime.MinValue;
            try
            {
                _time = DateTime.ParseExact(time, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _time = DateTime.MinValue;
            }

            return Ok(Program.api_report.getStatisticsCountPersonForDevice(_time));
        }




        [HttpGet]
        [Route("getStatisticsPersonForDeviceDates")]
        public IActionResult countPersonForDevice(string begin, string end)
        {
            DateTime _timebegin = DateTime.MinValue;
            try
            {
                _timebegin = DateTime.ParseExact(begin, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _timebegin = DateTime.MinValue;
            }

            DateTime _timeend = DateTime.MinValue;
            try
            {
                _timeend = DateTime.ParseExact(end, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _timeend = DateTime.MinValue;
            }

            return Ok(JsonConvert.SerializeObject(Program.api_report.getStatisticsPersonForDevice(_timebegin, _timeend)));
        }

        /*[HttpGet]
        [Route("countPersonForLevelDate")]
        public IActionResult countPersonForLevelDate(string time)
        {
            DateTime _time = DateTime.MinValue;
            try
            {
                _time = DateTime.ParseExact(time, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _time = DateTime.MinValue;
            }

            return Ok(Program.api_report.getStatisticsPersonForLevelDate(_time));
        }*/

        [HttpGet]
        [Route("getStatisticsPersonForLevel")]
        public IActionResult countPersonForLevelDateV2(string time)
        {
            DateTime _time = DateTime.MinValue;
            try
            {
                _time = DateTime.ParseExact(time, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _time = DateTime.MinValue;
            }

            return Ok(Program.api_report.getStatisticsCountPersonForLevel(_time));
        }

        [HttpGet]
        [Route("getStatisticsPersonForLevelDates")]
        public IActionResult countPersonForLevel(string begin, string end)
        {
            DateTime _timebegin = DateTime.MinValue;
            try
            {
                _timebegin = DateTime.ParseExact(begin, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _timebegin = DateTime.MinValue;
            }

            DateTime _timeend = DateTime.MinValue;
            try
            {
                _timeend = DateTime.ParseExact(end, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _timeend = DateTime.MinValue;
            }

            return Ok(JsonConvert.SerializeObject(Program.api_report.getStatisticsPersonForLevel(_timebegin, _timeend)));
        }

        /* [HttpGet]
         [Route("countPersonForGenderDate")]
         public IActionResult countPersonForGenderDate(string time)
         {
             DateTime _time = DateTime.MinValue;
             try
             {
                 _time = DateTime.ParseExact(time, "dd-MM-yyyy", null);
             }
             catch (Exception e)
             {
                 _time = DateTime.MinValue;
             }

             return Ok(Program.api_report.getStatisticsPersonForGenderDate(_time));
         }*/
        [HttpGet]
        [Route("getStatisticsPersonForGender")]
        public IActionResult countPersonForGenderDateV2(string time)
        {
            DateTime _time = DateTime.MinValue;
            try
            {
                _time = DateTime.ParseExact(time, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _time = DateTime.MinValue;
            }

            return Ok(Program.api_report.getStatisticsCountPersonForGender(_time));
        }

        [HttpGet]
        [Route("getStatisticsPersonForGenderDates")]
        public IActionResult countPersonForGender(string begin, string end)
        {
            DateTime _timebegin = DateTime.MinValue;
            try
            {
                _timebegin = DateTime.ParseExact(begin, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _timebegin = DateTime.MinValue;
            }

            DateTime _timeend = DateTime.MinValue;
            try
            {
                _timeend = DateTime.ParseExact(end, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                _timeend = DateTime.MinValue;
            }

            return Ok(JsonConvert.SerializeObject(Program.api_report.getStatisticsPersonForGender(_timebegin, _timeend)));
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

            return Ok(Program.api_report.getCountHourV2(time_input));

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

            return Ok(Program.api_report.getCountDatesV2(time_begin, time_end));
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

            return Ok(Program.api_report.getPersonsHoursV2(time_input));

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

            return Ok(Program.api_report.getCountPersonForDateV2(time_begin, time_end));
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

            return Ok(Program.api_report.getCountWithDeviceV2(time_input));

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

            return Ok(Program.api_report.getCountWithDeviceForDatesV2(time_begin, time_end));
        }

        [HttpGet]
        [Route("showPlotLevel")]
        public IActionResult showPlotPersonWithLevel(string time)
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

            return Ok(Program.api_report.showPlotLevelV2(time_input));

        }

        [HttpGet]
        [Route("showPlotLevelForDates")]
        public IActionResult showPlotLevelForDates(string begin, string end)
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

            return Ok(Program.api_report.showPlotLevelForDatesV2(time_begin, time_end));
        }

        [HttpGet]
        [Route("showPlotGender")]
        public IActionResult showPlotPersonWithGender(string time)
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

            return Ok(Program.api_report.showPlotGenderV2(time_input));

        }

        [HttpGet]
        [Route("showPlotGenderForDates")]
        public IActionResult showPlotGenderForDates(string begin, string end)
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

            return Ok(Program.api_report.showPlotGenderForDatesV2(time_begin, time_end));
        }

        //[HttpGet]
        //[Route("statisticsPersonForDevice")]
        //public IActionResult statisticsPersonForDevice(string time)
        //{
        //    DateTime m_time = DateTime.MinValue;
        //    try
        //    {
        //        m_time = DateTime.ParseExact(time, "dd-MM-yyyy", null);
        //        return Ok(Program.api_report.getStatisticsPersonForDevice(m_time));
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e.ToString());
        //        return Ok(new List<ItemPersonForDevice>());
        //    }
        //}
    }
}
