namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftsRepository
{
  void Add(Draft draft);

  void Update(Draft draft);

  Task<Draft?> GetByIdAsync(Guid draftId, CancellationToken cancellationToken);

  Task<Movie?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken);
}
