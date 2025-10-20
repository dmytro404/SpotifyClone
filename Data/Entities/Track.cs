namespace SpotifyClone.Data.Entities
{
    public class Track
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public string Url { get; set; } = ""; 
        public TimeSpan Duration { get; set; }
        public DateTime ReleaseDate { get; set; }

        public List<PlaylistTrack> PlaylistTracks { get; set; } = new();
        public List<Like> Likes { get; set; } = new();
    }
}
