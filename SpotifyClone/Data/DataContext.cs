using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data.Entities;
using SpotifyClone.Services.Kdf;

namespace SpotifyClone.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Genre> Genres { get; set; }

        public DataContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Relations

            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId);

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
                .WithOne(t => t.Album)
                .HasForeignKey(t => t.AlbumId);

            modelBuilder.Entity<Genre>()
                .HasMany(g => g.Tracks)
                .WithOne(t => t.Genre)
                .HasForeignKey(t => t.GenreId);

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Accesses);

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.Role)
                .WithMany()
                .HasForeignKey(ua => ua.RoleId);

            modelBuilder.Entity<UserAccess>()
                .HasIndex(ua => ua.Login)
                .IsUnique();

            #endregion

            #region Keys

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<UserAccess>()
                .Property(ua => ua.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Album>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Track>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Genre>()
                .Property(g => g.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Playlist>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            #endregion

            #region Indexes

            modelBuilder.Entity<Track>()
                .HasIndex(t => new { t.Title, t.Artist });

            modelBuilder.Entity<Album>()
                .HasIndex(a => a.Title);

            modelBuilder.Entity<Genre>()
                .HasIndex(g => g.Name)
                .IsUnique();

            #endregion

            #region Seed

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = "Admin",
                    Description = "System Root Administrator",
                    CanCreate = true,
                    CanRead = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new UserRole
                {
                    Id = "Guest",
                    Description = "Self Registered User",
                    CanCreate = false,
                    CanRead = false,
                    CanUpdate = false,
                    CanDelete = false
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Default Administrator",
                    Email = "admin@spotifyclone.dev",
                    CreatedAt = new DateTime(2025, 10, 25)
                }
            );

            modelBuilder.Entity<UserAccess>().HasData(
                new UserAccess
                {
                    Id = Guid.Parse("09DF387C-7050-4B76-9DB9-564EC352FD44"),
                    UserId = 1,
                    RoleId = "Admin",
                    Login = "Admin",
                    Salt = "4506C746-8FDD-4586-9BF4-95D6933C3B4F",
                    Dk = "F06BAC5028A11CE930866DFC16B8521EAE2F29311EE62C3649CD436D33D0AED8"
                }
            );

            #endregion
        }
    }
}