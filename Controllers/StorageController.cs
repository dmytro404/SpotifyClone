using SpotifyClone.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyClone.Controllers
{
    public class StorageController(IStorageService storageService) : Controller
    {
        private readonly IStorageService _storageService = storageService;
        public IActionResult Item(String id)
        {
            String ext = Path.GetExtension(id);
            String mimeType = ext switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
            var bytes = _storageService.Load(id);
            return bytes == null ? NotFound() : File(bytes, mimeType);
        }
    }
}
