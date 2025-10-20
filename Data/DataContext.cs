using SpotifyClone.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace SpotifyClone.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<Like> Likes { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Relations

            // User - Playlist (1 ко многим)
            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId);

            // Playlist - Track (многие ко многим через PlaylistTrack)
            modelBuilder.Entity<PlaylistTrack>()
                .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.Playlist)
                .WithMany(p => p.PlaylistTracks)
                .HasForeignKey(pt => pt.PlaylistId);

            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.Track)
                .WithMany(t => t.PlaylistTracks)
                .HasForeignKey(pt => pt.TrackId);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Track)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.TrackId);

            modelBuilder.Entity<Album>()
                .HasMany(a => a.Tracks)
                .WithOne()
                .HasForeignKey(t => t.Album);

            #endregion

            #region Indexes

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Track>()
                .HasIndex(t => new { t.Title, t.Artist });

            modelBuilder.Entity<Album>()
                .HasIndex(a => a.Title);

            #endregion

            #region Seed
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Login = "admin",
                    Email = "admin@spotifyclone.dev",
                    PasswordHash = "hashed_admin",
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Album>().HasData(
                new Album
                {
                    Id = 1,
                    Title = "Default Album",
                    Artist = "System",
                    CoverUrl = "/images/default_cover.png",
                    ReleaseDate = DateTime.UtcNow
                }
            );

            #endregion
        }
    }
}
