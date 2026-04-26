using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Home;
using SpotifyClone.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Services.Search;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/albums")]
    [ApiController]
    public class AlbumsController(
        DataContext dataContext,
        ISearchService searchService
    ) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            var query = _dataContext.Albums.AsQueryable();

            query = searchService.ApplySearch(query, search, "Title", "Artist");

            var data = query.Select(a => new {
                a.Id,
                a.Title,
                a.Artist,
                a.CoverUrl,
                ReleaseDate = a.ReleaseDate.ToShortDateString()
            }).ToList();

            return Ok(new { status = new { isOk = true }, data });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var album = _dataContext.Albums
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Artist,
                    a.CoverUrl,
                    ReleaseDate = a.ReleaseDate.ToShortDateString()
                })
                .FirstOrDefault();

            if (album == null)
            {
                return NotFound(new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code });
            }

            return Ok(new { status = new { isOk = true }, data = album });
        }

        [HttpPost("add")]
        public object AddAlbum(AdminAlbumFormModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Artist))
            {
                return new { status = RestStatus.Status400.Phrase, code = RestStatus.Status400.Code };
            }

            string coverUrl = SaveCover(model.Cover);

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

        [HttpPut("update/{id}")]
        public object UpdateAlbum(int id, AdminAlbumFormModel model)
        {
            var album = _dataContext.Albums.Find(id);
            if (album == null)
            {
                return new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };
            }

            album.Title = model.Title ?? album.Title;
            album.Artist = model.Artist ?? album.Artist;
            album.ReleaseDate = model.ReleaseDate;

            if (model.Cover != null && model.Cover.Length > 0)
            {
                album.CoverUrl = SaveCover(model.Cover);
            }

            try
            {
                _dataContext.SaveChanges();
                return new { status = RestStatus.Status200.Phrase, code = RestStatus.Status200.Code, data = album };
            }
            catch (Exception ex)
            {
                return new { status = ex.Message, code = RestStatus.Status500.Code };
            }
        }

        [HttpDelete("delete/{id}")]
        public object DeleteAlbum(int id)
        {
            var album = _dataContext.Albums.Find(id);
            if (album == null)
            {
                return new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };
            }

            _dataContext.Albums.Remove(album);

            try
            {
                _dataContext.SaveChanges();
                return new { status = RestStatus.Status200.Phrase, code = RestStatus.Status200.Code };
            }
            catch (Exception ex)
            {
                return new { status = ex.Message, code = RestStatus.Status500.Code };
            }
        }

        private string SaveCover(IFormFile? cover)
        {
            if (cover == null || cover.Length == 0) return "";

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "albums");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cover.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            cover.CopyTo(stream);

            return "/uploads/albums/" + fileName;
        }
    }
}