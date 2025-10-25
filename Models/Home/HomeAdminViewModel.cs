using SpotifyClone.Data.Entities;

namespace SpotifyClone.Models.Home
{
    public class HomeAdminViewModel
    {
        public List<Album> Albums { get; set; } = new();
        public List<TrackAdminViewModel> Tracks { get; set; } = new();

    }
    public class TrackAdminViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int AlbumId { get; set; }
        public string AlbumTitle { get; set; } = "";
        public TimeSpan Duration { get; set; }
    }


}