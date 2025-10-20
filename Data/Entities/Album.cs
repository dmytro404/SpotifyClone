namespace SpotifyClone.Data.Entities
{
    public class Album
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public DateTime ReleaseDate { get; set; }

        public List<Track> Tracks { get; set; } = new();
    }
}
