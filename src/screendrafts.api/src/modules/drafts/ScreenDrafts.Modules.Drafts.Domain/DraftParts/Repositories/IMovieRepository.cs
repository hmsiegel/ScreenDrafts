namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;

public interface IMovieRepository : IRepository<Movie>
{
  Task<Movie?> GetByIdAsync(Guid id, CancellationToken ct);
  Task<Movie?> GetByImdbIdAsync(string imdbId, CancellationToken ct);
  Task<Movie?> GetByTmdbIdAsync(int tmdbId, CancellationToken ct);
  Task<bool> ExistsAsync(Guid id, CancellationToken ct);
  Task<bool> ExistsByImdbIdAsync(string imdbId, CancellationToken ct);
  Task<bool> ExistsByTmdbIdAsync(int tmdbId, CancellationToken ct);
}
