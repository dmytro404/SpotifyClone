namespace SpotifyClone.Services.Search
{
    public interface ISearchService
    {
        IQueryable<T> ApplySearch<T>(IQueryable<T> query, string? searchTerm, params string[] properties) where T : class;
    }
}