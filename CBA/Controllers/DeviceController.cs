using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
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
        [Route("createDevice")]
        public async Task<IActionResult> CreateDeviceAsync([FromHeader] string token, HttpItemDevice device)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(device.code) || string.IsNullOrEmpty(device.name))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_device.createDevice(device.code, device.name, device.des);
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
        [Route("editDevice")]
        public async Task<IActionResult> EditDeviceAsync([FromHeader] string token, HttpItemDevice device)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(device.code) || string.IsNullOrEmpty(device.name))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_device.editDevice(device.code, device.name, device.des);
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
        [Route("{code}/deleteDevice")]
        public async Task<IActionResult> DeleteDeviceAsync([FromHeader] string token, string code)
        {
            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest();
                }
                bool flag = await Program.api_device.deleteDevice(code);
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
