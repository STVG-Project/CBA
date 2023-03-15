using CBA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                if (string.IsNullOrEmpty(group.code) || string.IsNullOrEmpty(group.name))
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
        [Route("{code}/cleanPerson")]
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
            public string codeSystem { get; set; } = "";
        }
        [HttpPost]
        [Route("editPerson")]
        public async Task<IActionResult> CreatePersonAsync([FromHeader] string token, HttpItemPerson person)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
               
                bool flag = await Program.api_person.editPerson(person.code, person.name, person.codeSystem);
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
        public IActionResult ListAllPerson()
        {
            return Ok(Program.api_person.getListPerson());

        }




        public class HttpItemFace
        {

            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public string device { get; set; } = "";
            public string codeSystem { get; set; } = "";
            public byte[] image { get; set; } = new byte[0];
        }
        [HttpPost]
        [Route("createFace")]
        public async Task<IActionResult> CreateFaceAsync([FromHeader] string token,  HttpItemFace face)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                //byte [] array = System.Text.Encoding.UTF8.GetBytes(face.image);
                //Console.WriteLine(array.ToString());
                bool flag = await Program.api_face.createFace(face.age, face.gender, face.image,face.device, face.codeSystem);
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
        [Route("getListFace")]
        public IActionResult ListFace()
        {
            return Ok(Program.api_face.getListFace());

        }
    }
}
