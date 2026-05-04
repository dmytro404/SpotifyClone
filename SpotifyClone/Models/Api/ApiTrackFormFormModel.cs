using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyClone.Models.Api
{
    public class ApiTrackFormModel
    {
        [FromForm(Name = "track-title")]
        public string Title { get; set; } = null!;

        [FromForm(Name = "track-artist")]
        public string Artist { get; set; } = null!;

        [FromForm(Name = "track-album-id")]
        public int AlbumId { get; set; }

        [FromForm(Name = "track-genre-id")]
        public int GenreId { get; set; }

        [FromForm(Name = "track-file")]
        public IFormFile? File { get; set; } = null!;
    }
}