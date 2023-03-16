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
    }
}
