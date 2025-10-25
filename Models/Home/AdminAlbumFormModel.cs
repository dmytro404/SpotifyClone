using Microsoft.AspNetCore.Mvc;

namespace SpotifyClone.Models.Home
{
    public class AdminAlbumFormModel
    {
        [FromForm(Name = "album-title")]
        public string Title { get; set; } = null!;

        [FromForm(Name = "album-artist")]
        public string Artist { get; set; } = null!;

        [FromForm(Name = "album-cover")]
        public IFormFile? Cover { get; set; }

        [FromForm(Name = "album-release-date")]
        public DateTime ReleaseDate { get; set; } = DateTime.Now;
    }
}