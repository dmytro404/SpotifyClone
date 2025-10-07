using SpotifyClone.Data.Entities;


namespace SpotifyClone.Models.Home
{
    public class HomeAdminViewModel
    {
        public IEnumerable<Genre> Genres { get; set; } = [];
    }
}
