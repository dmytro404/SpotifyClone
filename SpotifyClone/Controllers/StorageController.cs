using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Services.Storage;

namespace SpotifyClone.Controllers
{
    [Route("storage")]
    public class StorageController(IStorageService storageService) : Controller
    {
        private readonly IStorageService _storageService = storageService;

        // GET /storage/item/{id}
        [HttpGet("item/{id}")]
        public IActionResult Item(string id)
        {
            string ext = Path.GetExtension(id).ToLowerInvariant();

            string mimeType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".flac" => "audio/flac",
                _ => "application/octet-stream"
            };

            var bytes = _storageService.Load(id);
            return bytes == null ? NotFound() : File(bytes, mimeType);
        }

        // POST /storage/upload
        [HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                string filename = _storageService.Save(file);
                return Ok(new { file = filename, url = $"/storage/item/{filename}" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}