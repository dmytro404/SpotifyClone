using SpotifyClone.Data.Entities;

namespace SpotifyClone.Models.Home
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Genre> Genres { get; set; } = [];
    }
}
