using System;
using CBA;
using Microsoft.AspNetCore.Mvc;

namespace ServerWater.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileControlller : ControllerBase
    {
        private readonly ILogger<FileControlller> _logger;

        public FileControlller(ILogger<FileControlller> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("image/{code}")]
        public IActionResult GetImage(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }
            byte[]? data = Program.api_file.readFile(code);
            if (data == null)
            {
                return BadRequest(code);
            }
            return File(data!, "image/jpeg");
        }
    }
}

