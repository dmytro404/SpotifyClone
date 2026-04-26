using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Home;
using SpotifyClone.Models.Rest;
using SpotifyClone.Services.Auth;
using SpotifyClone.Services.Search;
using System.Diagnostics;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/tracks")]
    [ApiController]
    public class TracksController(
        DataContext dataContext,
        ISearchService searchService,
        IAuthService authService
    ) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            var query = _dataContext.Tracks.AsQueryable();

            query = searchService.ApplySearch(query, search, "Title", "Artist");

            var data = query.Select(t => new {
                t.Id,
                t.Title,
                t.Artist,
                t.Url,
                t.Duration,
                t.AlbumId,
                LikesCount = t.Likes.Count
            }).ToList();

            return Ok(new { status = new { isOk = true }, data });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var track = _dataContext.Tracks
                .Where(t => t.Id == id)
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.Artist,
                    t.Url,
                    t.Duration,
                    t.AlbumId,
                    LikesCount = t.Likes.Count
                })
                .FirstOrDefault();

            if (track == null)
            {
                return NotFound(new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code });
            }

            return Ok(new { status = new { isOk = true }, data = track });
        }

        [HttpPost("add")]
        public object AddTrack(AdminTrackFormModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Artist))
            {
                return new { status = RestStatus.Status400.Phrase, code = RestStatus.Status400.Code };
            }

            var albumExists = _dataContext.Albums.Any(a => a.Id == model.AlbumId);
            if (!albumExists)
            {
                return new { status = "Album " + RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };
            }

            var genreExists = _dataContext.Genres.Any(g => g.Id == model.GenreId);
            if (!genreExists)
            {
                return new { status = "Genre " + RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };
            }

            string trackUrl = "";
            TimeSpan duration = TimeSpan.Zero;

            if (model.File != null && model.File.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.File.CopyTo(stream);
                    }

                    trackUrl = "/uploads/tracks/" + fileName;

                    using (var tfile = TagLib.File.Create(filePath))
                    {
                        duration = tfile.Properties.Duration;
                    }
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
                GenreId = model.GenreId,
                Url = trackUrl,
                ReleaseDate = DateTime.Now,
                Duration = duration
            };

            _dataContext.Tracks.Add(track);
            _dataContext.SaveChanges();

            return new { status = RestStatus.Status200.Phrase, code = RestStatus.Status200.Code, data = track };
        }

        [HttpPut("update/{id}")]
        public object UpdateTrack(int id, AdminTrackFormModel model)
        {
            var track = _dataContext.Tracks.Find(id);
            if (track == null)
            {
                return new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };
            }

            track.Title = model.Title ?? track.Title;
            track.Artist = model.Artist ?? track.Artist;
            track.AlbumId = model.AlbumId != 0 ? model.AlbumId : track.AlbumId;

            if (model.File != null && model.File.Length > 0)
            {
                track.Url = SaveTrackFile(model.File);
            }

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

        [HttpDelete("delete/{id}")]
        public object DeleteTrack(int id)
        {
            var track = _dataContext.Tracks.Find(id);
            if (track == null)
            {
                return new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };
            }

            _dataContext.Tracks.Remove(track);

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

        [HttpPost("like/{id}")]
        public object ToggleLike(int id)
        {
            var user = authService.GetAuth<User>();
            if (user == null)
            {
                return new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code };
            }

            var like = _dataContext.Likes
                .FirstOrDefault(l => l.TrackId == id && l.UserId == user.Id);

            if (like == null)
            {
                _dataContext.Likes.Add(new Like { TrackId = id, UserId = user.Id });
            }
            else
            {
                _dataContext.Likes.Remove(like);
            }

            try
            {
                _dataContext.SaveChanges();
                return new { status = RestStatus.Status200.Phrase, code = RestStatus.Status200.Code, isLiked = (like == null) };
            }
            catch (Exception ex)
            {
                return new { status = ex.Message, code = RestStatus.Status500.Code };
            }
        }

        private string SaveTrackFile(IFormFile? file)
        {
            if (file == null || file.Length == 0) return "";

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tracks");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return "/uploads/tracks/" + fileName;
        }
    }
}