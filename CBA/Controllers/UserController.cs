using CBA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        //For User
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        public class ItemLoginUser
        {
            public string username { get; set; } = "";
            public string password { get; set; } = "";
        }


        [HttpPost]
        [Route("login")]
        public IActionResult Login(ItemLoginUser item)
        {
            return Ok(Program.api_user.login(item.username, item.password));
        }

        public class ItemUser
        {
            public string user { get; set; } = "";
            public string username { get; set; } = "";
            public string password { get; set; } = "";
            public string des { get; set; } = "";
            public string role { get; set; } = "";
            public string displayName { get; set; } = "";
            public string phoneNumber { get; set; } = "";
        }

        public class ItemUserV2
        {
            public string user { get; set; } = "";
            public string password { get; set; } = "";
            public string des { get; set; } = "";
            public string role { get; set; } = "";
            public string displayName { get; set; } = "";
            public string phoneNumber { get; set; } = "";
        }

        [HttpPost]
        [Route("createUser")]
        public async Task<IActionResult> CreateUserAsync([FromHeader] string token, ItemUser user)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                bool flag = await Program.api_user.createUserAsync(token, user.user, user.username, user.password, user.displayName, user.phoneNumber, user.des, user.role);
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
        [Route("editUser")]
        public async Task<IActionResult> editUserAsync([FromHeader] string token, ItemUserV2 user)
        {

            bool flag = await Program.api_user.editUserAsync(token, user.user, user.password, user.displayName, user.phoneNumber, user.des, user.role);
            if (flag)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpDelete]
        [Route("{code}/deleteUser")]
        public async Task<IActionResult> deleteUserAsync([FromHeader] string token, string code)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                bool flag = await Program.api_user.deleteUserAsync(token, code);
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
        [Route("addAvatar")]
        public async Task<IActionResult> addAvatarAsync([FromHeader] string token, IFormFile image)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                image.CopyTo(ms);
                string code = await Program.api_user.setAvatarAsync(token, ms.ToArray());
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest();
                }
                else
                {
                    return Ok(code);
                }
            }

        }

        [HttpGet]
        [Route("getListUser")]
        public IActionResult GetListUser([FromHeader] string token)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                return Ok(Program.api_user.listUser(token));
            }
            else
            {
                return Unauthorized();
            }
            //return Ok(JsonConvert.SerializeObject(Program.api_user.listUser(token)));
        }

        [HttpGet]
        [Route("getLogPerson")]
        public IActionResult detectBlackList(string group)
        {
            return Ok(Program.api_user.detectBlackList(group));

        }


        //For role

        public class HttpItemRole
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
            public string note { get; set; } = "";
        }
        [HttpGet]
        [Route("listRole")]
        public IActionResult ListRole([FromHeader] string token)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                return Ok(Program.api_role.getListRole());
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPost]
        [Route("createRole")]
        public async Task<IActionResult> AddRoleAsync([FromHeader] string token, HttpItemRole role)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(role.code) || string.IsNullOrEmpty(role.name))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_role.createRoleAsync(role.code, role.name, role.des, role.note);
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
        [Route("editRole")]
        public async Task<IActionResult> EditRoleAsync([FromHeader] string token, HttpItemRole role)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(role.code) || string.IsNullOrEmpty(role.name))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_role.editRoleAsync(role.code, role.name, role.des, role.note);
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
        [Route("{code}/deleteRole")]
        public async Task<IActionResult> DeleteRoleAsync([FromHeader] string token, string code)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_role.deleteRoleAsync(code);
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

    }
}
