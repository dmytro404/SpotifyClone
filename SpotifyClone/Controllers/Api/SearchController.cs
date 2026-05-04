using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Rest;
using System.Security.Claims;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/search")]
    [ApiController]
    public class SearchController(DataContext dataContext) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        private UserRole? GetCurrentRole()
        {
            var roleId = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleId)) return null;
            return _dataContext.UserRoles.FirstOrDefault(r => r.Id == roleId);
        }

        private static string BuildArtistId(string value)
        {
            var normalizedCharacters = value
                .Trim()
                .ToLowerInvariant()
                .Select(character => char.IsLetterOrDigit(character) ? character : '-')
                .ToArray();
            var slug = string.Join("-", new string(normalizedCharacters).Split('-', StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(slug) ? "unknown-artist" : slug;
        }

        [HttpGet]
        public IActionResult Search([FromQuery] string? q)
        {
            var searchTerm = q?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Ok(new { status = RestStatus.Status200, albums = Array.Empty<object>(), artists = Array.Empty<object>(), tracks = Array.Empty<object>() });
            }

            var normalizedSearchTerm = searchTerm.ToLowerInvariant();
            var hasYearSearch = int.TryParse(searchTerm, out var searchYear);

            var albums = _dataContext.Albums
                .Where(album =>
                    album.Title.ToLower().Contains(normalizedSearchTerm) ||
                    album.Artist.ToLower().Contains(normalizedSearchTerm) ||
                    (hasYearSearch && album.ReleaseDate.Year == searchYear))
                .OrderByDescending(album => album.ReleaseDate)
                .ThenByDescending(album => album.Id)
                .Select(album => new
                {
                    album.Id,
                    album.Title,
                    album.Artist,
                    album.CoverUrl,
                    ReleaseDate = album.ReleaseDate.ToShortDateString()
                })
                .ToList();

            var artistAlbums = _dataContext.Albums
                .Where(album =>
                    album.Artist.ToLower().Contains(normalizedSearchTerm) ||
                    (hasYearSearch && album.ReleaseDate.Year == searchYear))
                .ToList();

            var artists = artistAlbums
                .GroupBy(album => album.Artist)
                .Select(group =>
                {
                    var latestAlbum = group.OrderByDescending(album => album.ReleaseDate).First();
                    return new
                    {
                        Id = BuildArtistId(group.Key),
                        Name = group.Key,
                        AlbumCount = group.Count(),
                        LatestRelease = latestAlbum.ReleaseDate.ToShortDateString(),
                        CoverUrl = latestAlbum.CoverUrl
                    };
                })
                .OrderByDescending(artist => artist.AlbumCount)
                .ThenBy(artist => artist.Name)
                .ToList();

            var canReadTracks = GetCurrentRole()?.CanRead == true;
            var tracks = canReadTracks
                ? _dataContext.Tracks
                    .Where(track =>
                        track.Title.ToLower().Contains(normalizedSearchTerm) ||
                        track.Artist.ToLower().Contains(normalizedSearchTerm) ||
                        track.Album.Title.ToLower().Contains(normalizedSearchTerm) ||
                        track.Genre.Name.ToLower().Contains(normalizedSearchTerm) ||
                        (hasYearSearch && track.ReleaseDate.Year == searchYear))
                    .OrderByDescending(track => track.Id)
                    .Select(track => new
                    {
                        track.Id,
                        track.Title,
                        track.Artist,
                        track.Url,
                        track.Duration,
                        track.AlbumId,
                        AlbumTitle = track.Album.Title,
                        track.GenreId,
                        GenreName = track.Genre.Name,
                        LikesCount = track.Likes.Count
                    })
                    .ToList<object>()
                : [];

            return Ok(new
            {
                status = RestStatus.Status200,
                albums,
                artists,
                tracks
            });
        }
    }
}