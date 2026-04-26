using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data.Entities;

namespace SpotifyClone.Services.Search
{
    public class SearchService : ISearchService
    {
        public IQueryable<T> ApplySearch<T>(IQueryable<T> query, string? searchTerm, params string[] properties) where T : class
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var s = searchTerm.ToLower();

            if (typeof(T) == typeof(Album))
            {
                return query.Where(item =>
                    EF.Property<string>(item, "Title").ToLower().Contains(s) ||
                    EF.Property<string>(item, "Artist").ToLower().Contains(s));
            }

            if (typeof(T) == typeof(Track))
            {
                return query.Where(item =>
                    EF.Property<string>(item, "Title").ToLower().Contains(s) ||
                    EF.Property<string>(item, "Artist").ToLower().Contains(s));
            }

            return query;
        }
    }
}