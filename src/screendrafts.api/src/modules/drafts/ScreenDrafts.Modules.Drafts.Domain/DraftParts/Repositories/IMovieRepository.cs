namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;

public interface IMovieRepository : IRepository<Movie, Guid>
{
  Task<Movie?> GetByImdbIdAsync(string imdbId, CancellationToken cancellationToken);
  Task<Movie?> GetByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken);
  Task<Movie?> GetByIgdbIdAsync(int igdbId, CancellationToken cancellationToken);
  Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken);
  Task<bool> ExistsByImdbIdAsync(string imdbId, CancellationToken cancellationToken);
  Task<bool> ExistsByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken);
  Task<bool> ExistsByIgdbIdAsync(int igdbId, CancellationToken cancellationToken);
  Task<HashSet<int>> GetExistingTmdbIdsAsync(IReadOnlyList<int> validTmdbIds, CancellationToken cancellationToken);
  Task<HashSet<int>> GetExistingIgdbIdsAsync(IReadOnlyList<int> validIgdbIds, CancellationToken cancellationToken);
}
