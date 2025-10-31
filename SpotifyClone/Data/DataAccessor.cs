using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data.Entities;
using SpotifyClone.Services.Kdf;

namespace SpotifyClone.Data
{
    public class DataAccessor(DataContext dataContext, IKdfService kdfService)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;

        public UserAccess? Authenticate(string login, string password)
        {
            var userAccess = _dataContext.UserAccesses
                .Include(ua => ua.User)
                .Include(ua => ua.Role)
                .AsTracking()
                .FirstOrDefault(ua => ua.Login == login);

            if (userAccess == null) return null;

            string dk = _kdfService.Dk(password, userAccess.Salt);

            return string.Equals(dk, userAccess.Dk, StringComparison.OrdinalIgnoreCase) ? userAccess : null;
        }

        public User CreateUser(string name, string email, string login, string password, string roleId)
        {
            var role = _dataContext.UserRoles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) role = new UserRole { Id = roleId, Description = roleId };
            if (!_dataContext.UserRoles.Any(r => r.Id == roleId)) _dataContext.UserRoles.Add(role);

            var salt = Guid.NewGuid().ToString();
            var dk = _kdfService.Dk(password, salt);

            var user = new User { Name = name, Email = email, CreatedAt = DateTime.Now };
            _dataContext.Users.Add(user);
            _dataContext.SaveChanges();

            var access = new UserAccess
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = roleId,
                Login = login,
                Salt = salt,
                Dk = dk
            };
            _dataContext.UserAccesses.Add(access);
            _dataContext.SaveChanges();

            return user;
        }

        public Album AddAlbum(string title, string artist, string coverUrl, DateTime releaseDate)
        {
            var album = new Album
            {
                Title = title,
                Artist = artist,
                CoverUrl = coverUrl,
                ReleaseDate = releaseDate
            };
            _dataContext.Albums.Add(album);
            _dataContext.SaveChanges();
            return album;
        }

        public Album? GetAlbum(int id) =>
            _dataContext.Albums.Include(a => a.Tracks).FirstOrDefault(a => a.Id == id);

        public IEnumerable<Album> GetAlbums() =>
            _dataContext.Albums.Include(a => a.Tracks).AsEnumerable();

        public Track AddTrack(string title, string artist, string url, TimeSpan duration, DateTime releaseDate, int albumId, int genreId)
        {
            var track = new Track
            {
                Title = title,
                Artist = artist,
                Url = url,
                Duration = duration,
                ReleaseDate = releaseDate,
                AlbumId = albumId,
                GenreId = genreId
            };
            _dataContext.Tracks.Add(track);
            _dataContext.SaveChanges();
            return track;
        }

        public Track? GetTrack(int id) =>
            _dataContext.Tracks.Include(t => t.Album).Include(t => t.Genre).FirstOrDefault(t => t.Id == id);

        public IEnumerable<Track> GetTracks() =>
            _dataContext.Tracks.Include(t => t.Album).Include(t => t.Genre).AsEnumerable();
    }
}