namespace SpotifyClone.Data.Entities
{
    public class Song
    {
        public Guid Id { get; set; }
        public Guid GenreId { get; set; }
        public String Name { get; set; } = null!;
        public String? Slug { get; set; } = null!;
        public String? ImageUrl { get; set; } = null!;
        public String? FilePath { get; set; }
        public Guid ArtistId { get; set; }
        public User Artist { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }
}
