namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;

public interface IMovieRepository : IRepository<Movie>
{
  Task<Movie?> GetByIdAsync(Guid id, CancellationToken ct);
  Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
