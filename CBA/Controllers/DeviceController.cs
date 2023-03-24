using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(ILogger<DeviceController> logger)
        {
            _logger = logger;
        }
        //For device
        public class HttpItemDevice
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }
        [HttpGet]
        [Route("listDevice")]
        public IActionResult ListDevice([FromHeader] string token)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                return Ok(Program.api_device.getListDevice());
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPost]
        [Route("{code}/edit")]
        public async Task<IActionResult> DeleteDeviceAsync([FromHeader] string token, HttpItemDevice item)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(item.code))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_device.editDevice(item.code, item.name, item.des);
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
        [Route("{code}/delete")]
        public async Task<IActionResult> DeleteDeviceAsync([FromHeader] string token, string code)
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
    }
}
