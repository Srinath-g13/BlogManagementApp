using Microsoft.AspNetCore.Mvc;

namespace BlogManagementApp.Controllers
{
    [Route("file/[controller]")]
    [ApiController]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _Environment;

        public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
        {
            _Environment = environment;
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> FileUpload(IFormFile Upload)
        {
            try
            {
                if (Upload == null || Upload.Length == 0)
                {
                    return BadRequest(new { error = new { message = "No file uploaded." } });
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extensions = Path.GetExtension(Upload.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extensions) || !allowedExtensions.Contains(extensions))
                {
                    return BadRequest(new { error = new { message = "invalid return type" } });
                }
                var UniqueFileName = Guid.NewGuid().ToString() + extensions;
                var UploadsFile = Path.Combine(_Environment.WebRootPath, "uploads");
                if (!Directory.Exists(UploadsFile))
                {
                    Directory.CreateDirectory(UploadsFile);
                }
                var FilePath = Path.Combine(UploadsFile, UniqueFileName);
                using (var stream = new FileStream(FilePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }
                var response = new
                {
                    uploaded = true,
                    url = Url.Content($"~/Uploads/{UniqueFileName}")
                };
                return Ok(response);

            }
            catch (IOException ioEx)
            {
                return StatusCode(500, new { error = new { message = "An error occurred while saving the file. Please try again." } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = new { message = "An unexpected error occurred. Please try again later." } });
            }
        }
    }
}
