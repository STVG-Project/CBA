
using Microsoft.AspNetCore.Mvc;

namespace CBA.Controllers
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

        

        //public async Task<bool> createConfig(string lang, byte[] data)
        //{
        //    string path = "./Configs";
        //    string fileName = "DataBaseConfig" + ".json";
        //    try
        //    {
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //        if (data.Length > 0)
        //        {
        //            string link = Path.Combine(path, fileName);
        //            await File.WriteAllBytesAsync(link, data);
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
    }
}

