using SpotifyClone.Data.Entities;
using SpotifyClone.Services.Kdf;
using Microsoft.EntityFrameworkCore;

namespace SpotifyClone.Data
{
    public class DataAccessor(DataContext dataContext, IKdfService kdfService)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;

        public UserAccess? Authenticate(String login, String password)
        {
            var userAccess = _dataContext
               .UserAccesses
               .AsNoTracking()
               .Include(ua => ua.User)
               .Include(ua => ua.Role)
               .FirstOrDefault(ua => ua.Login == login);

            if (userAccess == null)
            {
                return null;
            }

            String dk = _kdfService.Dk(password, userAccess.Salt);
            if (dk != userAccess.Dk)
            {
                return null;
            }
            return userAccess;
        }

        public User? GetUserById(Guid id)
        {
            return _dataContext.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == id && u.DeleteAt == null);
        }

        public Genre? AddGenre(Genre genre)
        {
            _dataContext.Genres.Add(genre);
            _dataContext.SaveChanges();
            return genre;
        }
        public Genre? GetGenreByName(string name)
        {
            return _dataContext.Genres
                               .AsNoTracking()
                               .FirstOrDefault(g => g.Name.ToLower() == name.ToLower());
        }

        public IEnumerable<Genre> GetGenres()
        {
            return _dataContext
                    .Genres
                    .Where(g => g.DeletedAt == null)
                    .AsEnumerable();
        }
    }
}
