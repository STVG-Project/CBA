using CBA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using static CBA.APIs.MyPerson;

namespace GIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public class HttpItemGroup
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }
        [HttpGet]
        [Route("getListGroup")]
        public IActionResult ListGroup()
        {
            return Ok(Program.api_group.getListGroup());

        }

        [HttpGet]
        [Route("{group}/getListPerson")]
        public IActionResult ListPerson(string group)
        {
            return Ok(Program.api_group.getListPersonInGroup(group));

        }

        [HttpPost]
        [Route("createGroup")]
        public async Task<IActionResult> CreateGroupAsync([FromHeader] string token, HttpItemGroup group)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(group.code) || string.IsNullOrEmpty(group.name))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_group.createGroup(group.code, group.name, group.des);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("editGroup")]
        public async Task<IActionResult> EditGroupAsync([FromHeader] string token, HttpItemGroup group)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(group.code))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_group.editGroup(group.code, group.name, group.des);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete]
        [Route("{code}/deleteGroup")]
        public async Task<IActionResult> DeleteGroupAsync([FromHeader] string token, string code)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_group.deleteGroup(code);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        public class HttpItemAgeLevel
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
            public int low { get; set; } = 0;
            public int high { get; set; } = 0;
        }

        [HttpGet]
        [Route("getListAgeLevel")]
        public IActionResult ListAgeLevel()
        {
            return Ok(Program.api_age.getListAgeLevel());

        }

        [HttpPost]
        [Route("createAgeLevel")]
        public async Task<IActionResult> CreateAgeLevelAsync([FromHeader] string token, HttpItemAgeLevel level)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(level.code) || string.IsNullOrEmpty(level.name))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_age.createAgeLevel(level.code, level.name, level.des, level.low, level.high);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("editAgeLevel")]
        public async Task<IActionResult> EditAgeLevelAsync([FromHeader] string token, HttpItemAgeLevel level)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(level.code))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_age.editAgeLevel(level.code, level.name, level.des, level.low, level.high);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpDelete]
        [Route("{code}/deleteAgeLevel")]
        public async Task<IActionResult> DeleteAgeLevelAsync([FromHeader] string token, string code)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_age.deleteAgeLevel(code);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPut]
        [Route("{group}/setPerson")]
        public async Task<IActionResult> SetPersonAsync([FromHeader] string token, string person, string group)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {

                if (string.IsNullOrEmpty(person) || string.IsNullOrEmpty(group))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_group.SetPersonAsync(person, group);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPut]
        [Route("{group}/cleanPerson")]
        public async Task<IActionResult> RemoveUserAsync([FromHeader] string token, string person, string group)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {

                if (string.IsNullOrEmpty(person) || string.IsNullOrEmpty(group))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_group.cleanPersonAsync(person, group);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        // for person
        public class HttpItemPerson
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
            public string codeSystem { get; set; } = "";
        }
        [HttpPost]
        [Route("editPerson")]
        public async Task<IActionResult> CreatePersonAsync([FromHeader] string token, HttpItemPerson person)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
               
                bool flag = await Program.api_person.editPerson(person.code, person.name, person.des, person.codeSystem);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        [Route("getListPerson")]
        public IActionResult ListAllPerson(int page, int numPerson)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            ListPersonPage data = Program.api_person.getListPerson(page, numPerson);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine(string.Format("getReportPerson : {0}", elapsedTime));
           
            return Ok(data);

        }

        [HttpGet]
        [Route("getListPersonHistory")]
        public IActionResult getListPersonHistory(string begin, string end, int page, int numPerson)
        {
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
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
            ListInfoLogsPage data = Program.api_person.getListPersonHistory(time_begin, time_end, page, numPerson);
            //stopWatch.Stop();
            //TimeSpan ts = stopWatch.Elapsed;
            //string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            //Console.WriteLine(string.Format("getReportPerson : {0}", elapsedTime));
            //Log.Information(string.Format("getReportPerson : {0}", elapsedTime));
            return Ok(data);
           
        }

    }
}
