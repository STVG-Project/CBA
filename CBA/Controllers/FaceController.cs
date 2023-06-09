﻿using GIS.Controllers;
using Microsoft.AspNetCore.Mvc;
using static GIS.Controllers.HomeController;

namespace CBA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private readonly ILogger<FaceController> _logger;

        public FaceController(ILogger<FaceController> logger)
        {
            _logger = logger;
        }

        public class HttpCreateFace
        {

            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public string device { get; set; } = "";
            public string codeSystem { get; set; } = "";
            public IFormFile image { get; set; }
        }

        [HttpPost]
        [Route("createFace")]
        public async Task<IActionResult> CreateFaceAsync([FromHeader] string token, [FromForm] HttpCreateFace face)
        {

            long id = Program.api_user.checkUser(token);
            if (id >= 0)
            {
                /*
                byte[] array = System.Text.Encoding.UTF8.GetBytes(face.image);
                Console.WriteLine(array.ToString());
                bool flag = await Program.api_face.createFace(face.age, face.gender, array, face.device, face.codeSystem);
                if (flag)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
                */
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
