using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Home;
using SpotifyClone.Models.Rest;
using SpotifyClone.Services.Search;
using System.Security.Claims;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/tracks")]
    [ApiController]
    public class TracksController(
        DataContext dataContext,
        ISearchService searchService
    ) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        private UserRole? GetCurrentRole()
        {
            var roleId = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleId)) return null;
            return _dataContext.UserRoles.FirstOrDefault(r => r.Id == roleId);
        }

        private int ResolveGenreId(int requestedGenreId)
        {
            if (requestedGenreId != 0 && _dataContext.Genres.Any(g => g.Id == requestedGenreId))
                return requestedGenreId;

            var existingGenre = _dataContext.Genres.OrderBy(g => g.Id).FirstOrDefault();
            if (existingGenre != null) return existingGenre.Id;

            var defaultGenre = new Genre { Name = "Uncategorized" };
            _dataContext.Genres.Add(defaultGenre);
            _dataContext.SaveChanges();
            return defaultGenre.Id;
        }

        private static object ToTrackResponse(Track track) => new
        {
            track.Id,
            track.Title,
            track.Artist,
            track.Url,
            track.Duration,
            track.AlbumId,
            AlbumTitle = track.Album?.Title,
            track.GenreId,
            GenreName = track.Genre?.Name,
            LikesCount = track.Likes?.Count ?? 0
        };

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            var role = GetCurrentRole();
            if (role == null || !role.CanRead)
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            var query = _dataContext.Tracks
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .Include(t => t.Likes)
                .AsQueryable();
            query = searchService.ApplySearch(query, search, "Title", "Artist");

            var data = query.Select(t => new {
                    t.Id,
                    t.Title,
                    t.Artist,
                    t.Url,
                    t.Duration,
                    t.AlbumId,
                    AlbumTitle = t.Album.Title,
                    t.GenreId,
                    GenreName = t.Genre.Name,
                    LikesCount = t.Likes.Count
                })
                .ToList();

            return Ok(new { status = RestStatus.Status200, data });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var role = GetCurrentRole();
            if (role == null || !role.CanRead)
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            var track = _dataContext.Tracks
                .Include(t => t.Album)
                .Include(t => t.Genre)
                .Include(t => t.Likes)
                .Where(t => t.Id == id)
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.Artist,
                    t.Url,
                    t.Duration,
                    t.AlbumId,
                    AlbumTitle = t.Album.Title,
                    t.GenreId,
                    GenreName = t.Genre.Name,
                    LikesCount = t.Likes.Count
                })
                .FirstOrDefault();

            if (track == null)
                return NotFound(new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code });

            return Ok(new { status = RestStatus.Status200, data = track });
        }

        [HttpPost("add")]
        public object AddTrack(AdminTrackFormModel model)
        {
            var role = GetCurrentRole();
            if (role == null || !role.CanCreate)
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            if (model == null || string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Artist))
                return new { status = RestStatus.Status400.Phrase, code = RestStatus.Status400.Code };

            var albumExists = _dataContext.Albums.Any(a => a.Id == model.AlbumId);
            if (!albumExists)
                return new { status = "Album " + RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };

            var genreId = ResolveGenreId(model.GenreId);

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
                        model.File.CopyTo(stream);

                    trackUrl = "/uploads/tracks/" + fileName;

                    using (var tfile = TagLib.File.Create(filePath))
                        duration = tfile.Properties.Duration;
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
                GenreId = genreId,
                Url = trackUrl,
                ReleaseDate = DateTime.Now,
                Duration = duration
            };

            _dataContext.Tracks.Add(track);
            _dataContext.SaveChanges();
            _dataContext.Entry(track).Reference(t => t.Album).Load();
            _dataContext.Entry(track).Reference(t => t.Genre).Load();

            return new { status = RestStatus.Status200.Phrase, code = RestStatus.Status200.Code, data = ToTrackResponse(track) };
        }

        [HttpPut("update/{id}")]
        public object UpdateTrack(int id, AdminTrackFormModel model)
        {
            var role = GetCurrentRole();
            if (role == null || !role.CanUpdate)
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            var track = _dataContext.Tracks.Find(id);
            if (track == null)
                return new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };

            if (!string.IsNullOrWhiteSpace(model.Title)) track.Title = model.Title;
            if (!string.IsNullOrWhiteSpace(model.Artist)) track.Artist = model.Artist;

            if (model.AlbumId != 0)
            {
                var albumExists = _dataContext.Albums.Any(a => a.Id == model.AlbumId);
                if (!albumExists)
                    return new { status = "Album " + RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };

                track.AlbumId = model.AlbumId;
            }

            if (model.GenreId != 0)
                track.GenreId = ResolveGenreId(model.GenreId);

            if (model.File != null && model.File.Length > 0)
                track.Url = SaveTrackFile(model.File);

            try
            {
                _dataContext.SaveChanges();
                _dataContext.Entry(track).Reference(t => t.Album).Load();
                _dataContext.Entry(track).Reference(t => t.Genre).Load();
                return new { status = RestStatus.Status200.Phrase, code = RestStatus.Status200.Code, data = ToTrackResponse(track) };
            }
            catch (Exception ex)
            {
                return new { status = ex.Message, code = RestStatus.Status500.Code };
            }
        }

        [HttpDelete("delete/{id}")]
        public object DeleteTrack(int id)
        {
            var role = GetCurrentRole();
            if (role == null || !role.CanDelete)
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            var track = _dataContext.Tracks.Find(id);
            if (track == null)
                return new { status = RestStatus.Status404.Phrase, code = RestStatus.Status404.Code };

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

        [HttpGet("liked")]
        public IActionResult GetLikedTracks()
        {
            var role = GetCurrentRole();
            if (role == null || !role.CanRead)
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            var data = _dataContext.Likes
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ThenByDescending(l => l.Id)
                .Select(l => new {
                    l.Track.Id,
                    l.Track.Title,
                    l.Track.Artist,
                    l.Track.Url,
                    l.Track.Duration,
                    l.Track.AlbumId,
                    AlbumTitle = l.Track.Album.Title,
                    l.Track.GenreId,
                    GenreName = l.Track.Genre.Name,
                    LikesCount = l.Track.Likes.Count
                })
                .ToList();

            return Ok(new
            {
                status = RestStatus.Status200,
                trackIds = data.Select(t => t.Id).ToList(),
                data
            });
        }
        [HttpPost("like/{id}")]
        public object ToggleLike(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { status = RestStatus.Status401.Phrase, code = RestStatus.Status401.Code });

            var like = _dataContext.Likes
                .FirstOrDefault(l => l.TrackId == id && l.UserId == userId);

            if (like == null)
                _dataContext.Likes.Add(new Like { TrackId = id, UserId = userId, CreatedAt = DateTime.Now });
            else
                _dataContext.Likes.Remove(like);

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
