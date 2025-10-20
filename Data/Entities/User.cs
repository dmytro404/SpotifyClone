namespace SpotifyClone.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public List<Playlist> Playlists { get; set; } = new();
        public List<Like> Likes { get; set; } = new();
    }
}
