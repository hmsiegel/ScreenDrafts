namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftsRepository(DraftsDbContext dbContext) : IDraftsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Draft draft)
  {
    _dbContext.Drafts.Add(draft);
  }

  public async Task<Draft?> GetByIdAsync(Guid draftId, CancellationToken cancellationToken)
  {
    var draft = await _dbContext.Drafts
      .FirstOrDefaultAsync(d => d.Id.Value == draftId, cancellationToken);

    return draft;
  }

  public async Task<Movie?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken)
  {
    var movie = await _dbContext.Movies
      .FirstOrDefaultAsync(m => m.Id.Value == movieId, cancellationToken);

    return movie;
  }

  public void Update(Draft draft)
  {
    _dbContext.Drafts.Update(draft);
  }
}
