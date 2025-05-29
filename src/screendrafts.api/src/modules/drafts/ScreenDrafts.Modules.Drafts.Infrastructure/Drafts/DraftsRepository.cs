namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftsRepository(DraftsDbContext dbContext) : IDraftsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Draft draft)
  {
    var existingEntity = _dbContext.ChangeTracker.Entries<Draft>()
      .FirstOrDefault(e => e.Entity.Id == draft.Id);

    if (existingEntity is not null)
    {
      return;
    }

    _dbContext.Drafts.Add(draft);
  }

  public void Update(Draft draft)
  {
    _dbContext.Drafts.Update(draft);
  }

  public void AddMovie(Movie movie)
  {
    _dbContext.Movies.Add(movie);
  }

  public void AddCommissionerOverride(CommissionerOverride commissionerOverride)
  {
    _dbContext.CommissionerOverrides.Add(commissionerOverride);
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

  public async Task<Draft?> GetDraftWithDetailsAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    var draftWithDetails = await _dbContext.Drafts
      .Include(d => d.Drafters)
      .Include(d => d.Hosts)
      .Include(d => d.ReleaseDates)
      .Include(d => d.Picks)
      .FirstOrDefaultAsync(d => d.Id == draftId, cancellationToken);

    return draftWithDetails;
  }

  public Task<bool> MovieExistsAsync(string imdbId, CancellationToken cancellationToken)
  {
    return _dbContext.Movies.AnyAsync(m => m.ImdbId == imdbId, cancellationToken);
  }

  public void Delete(Draft draft)
  {
    _dbContext.Drafts.Remove(draft);
  }
}
