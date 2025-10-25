using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Home;
using SpotifyClone.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/tracks")]
    [ApiController]
    public class TracksController(
        DataContext dataContext
    ) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpPost("add")]
        public object AddTrack(AdminTrackFormModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Artist))
            {
                return new { status = RestStatus.Status400.Phrase, code = RestStatus.Status400.Code };
            }

            // Проверяем, что альбом существует
            var album = _dataContext.Albums.FirstOrDefault(a => a.Id == model.AlbumId);
            if (album == null)
            {
                return new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };
            }

            string trackUrl = "";
            if (model.File != null && model.File.Length > 0)
            {
                try
                {
                    // Папка для аудио внутри проекта
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    model.File.CopyTo(stream);

                    // Относительный путь для браузера
                    trackUrl = "/uploads/tracks/" + fileName;
                }
                catch (Exception ex)
                {
                    return new { status = ex.Message, code = RestStatus.Status500.Code };
                }
            }

            var track = new Track
            {
                Title = model.Title,
                Artist = model.Artist,
                AlbumId = model.AlbumId,
                Url = trackUrl,
                ReleaseDate = DateTime.Now,
                Duration = TimeSpan.Zero
            };

            _dataContext.Tracks.Add(track);

            try
            {
                _dataContext.SaveChanges();
                return new
                {
                    status = RestStatus.Status200.Phrase,
                    code = RestStatus.Status200.Code,
                    data = new { track.Id, track.Title, track.Url }
                };
            }
            catch (Exception ex)
            {
                return new { status = ex.Message, code = RestStatus.Status500.Code };
            }
        }
    }
}