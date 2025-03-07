namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftsRepository(DraftsDbContext dbContext) : IDraftsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Draft draft)
  {
    _dbContext.Drafts.Add(draft);
  }

  public async Task<Draft?> GetByIdAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    var draft = await _dbContext.Drafts
      .FirstOrDefaultAsync(d => d.Id == draftId, cancellationToken);

    return draft;
  }

  public async Task<Movie?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken)
  {
    var movie = await _dbContext.Movies
      .FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken);

    return movie;
  }

  public void Update(Draft draft)
  {
    _dbContext.Drafts.Update(draft);
  }

  public void AddMovie(Movie movie)
  {
    _dbContext.Movies.Add(movie);
  }

  public async Task<Draft?> GetDraftWithDetailsAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    var draftWithDetails = await  _dbContext.Drafts
      .Include(d => d.Drafters)
      .Include(d => d.Hosts)
      .Include(d => d.ReleaseDates)
      .Include(d => d.Picks)
      .FirstOrDefaultAsync(d => d.Id == draftId, cancellationToken);

    return draftWithDetails;
  }
}
