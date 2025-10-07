namespace SpotifyClone.Data.Entities
{
    public class Genre
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public String Slug { get; set; } = null!;
        public String ImageUrl { get; set; } = null!;
        public DateTime? DeletedAt { get; set; }
        public ICollection<Song> Songs { get; set; } = [];
    }
}
