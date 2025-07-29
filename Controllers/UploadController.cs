using Microsoft.AspNetCore.Mvc;

namespace BlogManagementApp.Controllers
{
    [Route("file/[controller]")]
    [ApiController]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _Environment;

        public UploadController(IWebHostEnvironment environment,ILogger<UploadController> logger)
        {
            _Environment = environment;
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> FileUpload(IFormFile Upload)
        {
            try
            {
                if(Upload == null || Upload.Length == 0)
                {
                    return BadRequest(new { error = new { message = "No file uploaded." } });
                }
                
            }
        }
    }
}
