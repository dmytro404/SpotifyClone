using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Home;
using SpotifyClone.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/albums")]
    [ApiController]
    public class AlbumsController(
        DataContext dataContext
    ) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpPost("add")]
        public object AddAlbum(AdminAlbumFormModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Artist))
            {
                return new { status = RestStatus.Status400.Phrase, code = RestStatus.Status400.Code };
            }

            string coverUrl = "";
            if (model.Cover != null && model.Cover.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "albums");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Cover.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    model.Cover.CopyTo(stream);

                    coverUrl = "/uploads/albums/" + fileName;
                }
                catch (Exception ex)
                {
                    return new { status = ex.Message, code = RestStatus.Status500.Code };
                }
            }

            var album = new Album
            {
                Title = model.Title,
                Artist = model.Artist,
                ReleaseDate = model.ReleaseDate,
                CoverUrl = coverUrl
            };

            _dataContext.Albums.Add(album);

            try
            {
                _dataContext.SaveChanges();
                return new
                {
                    status = RestStatus.Status200.Phrase,
                    code = RestStatus.Status200.Code,
                    data = new { album.Id, album.Title, album.CoverUrl }
                };
            }
            catch (Exception ex)
            {
                return new { status = ex.Message, code = RestStatus.Status500.Code };
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var albums = _dataContext.Albums
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Artist,
                    a.CoverUrl,
                    ReleaseDate = a.ReleaseDate.ToShortDateString()
                })
                .ToList();

            return Ok(new { status = new { isOk = true }, data = albums });
        }

    }
}