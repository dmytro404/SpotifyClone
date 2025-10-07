using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data.Entities;

namespace SpotifyClone.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccess>()
               .HasOne(ua => ua.User)
               .WithMany(u => u.Accesses)
               .HasForeignKey(ua => ua.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.Role)
                .WithMany()
                .HasForeignKey(ua => ua.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Artist)
                .WithMany(u => u.Songs)
                .HasForeignKey(s => s.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Genre)
                .WithMany(g => g.Songs)
                .HasForeignKey(s => s.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAccess>()
                .HasIndex(ua => ua.Login)
                .IsUnique();

            modelBuilder.Entity<Song>()
                .HasIndex(s => s.Slug)
                .IsUnique();

            modelBuilder.Entity<Genre>()
                .HasIndex(g => g.Slug)
                .IsUnique();

            #region seeding
            modelBuilder.Entity<UserRole>()
                .HasData(
                    new UserRole()
                    {
                        Id = "Admin",
                        Description = "System Root Administrator",
                        CanCreate = true,
                        CanDelete = true,
                        CanRead = true,
                        CanUpdate = true,
                    },
                    new UserRole()
                    {
                        Id = "Artist",
                        Description = "Firts added artist",
                        CanCreate = true,
                        CanUpdate = true,
                        CanRead = true,
                        CanDelete = false,
                    },
                    new UserRole()
                    {
                        Id = "Guest",
                        Description = "Self Registered User",
                        CanCreate = false,
                        CanDelete = false,
                        CanUpdate = false,
                        CanRead = false,
                    }
                    );
            modelBuilder.Entity<User>()
                .HasData(new User()
                {
                    Id = Guid.Parse("27745D91-2DAF-4088-8925-74E5F88BF415"),
                    Name = "Default Administrator",
                    Email = "admin@sptf.ua",
                    RegisteredAt = DateTime.UnixEpoch,
                });
            modelBuilder.Entity<UserAccess>()
                .HasData(new UserAccess()
                {
                    Id = Guid.Parse("09DF387C-7050-4B76-9DB9-564EC352FD44"),
                    UserId = Guid.Parse("27745D91-2DAF-4088-8925-74E5F88BF415"),
                    RoleId = "Admin",
                    Login = "Admin",
                    Salt = "4506C746-8FDD-4586-9BF4-95D6933C3B4F",
                    Dk = "2744FC45FF2F7CACD2EB"
                });
            #endregion
        }

    }
}
