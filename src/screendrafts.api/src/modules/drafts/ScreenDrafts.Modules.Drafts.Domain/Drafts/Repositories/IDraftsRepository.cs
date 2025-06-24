namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftsRepository : IRepository
{
  void Add(Draft draft);

  void Update(Draft draft);

  void AddMovie(Movie movie);

  void Delete(Draft draft);

  void AddCommissionerOverride(CommissionerOverride commissionerOverride);

  Task<Draft?> GetByIdAsync(DraftId draftId, CancellationToken cancellationToken);

  Task<Movie?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken);

  Task<Draft?> GetDraftWithDetailsAsync(DraftId draftId, CancellationToken cancellationToken);

  Task<bool> MovieExistsAsync(string imdbId, CancellationToken cancellationToken);

  Task<List<CommissionerOverride?>> GetCommissionerOverridesByDraftIdAsync(
    DraftId draftId, CancellationToken cancellationToken);
}
