namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftMovieRepository(GuestDraftsDbContext dbContext)
  : IGuestDraftMovieRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(GuestDraftMovie movie) => _dbContext.GuestDraftMovies.Add(movie);

  public Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    _dbContext.GuestDraftMovies.AnyAsync(m => m.PublicId == publicId, cancellationToken);

  public Task<GuestDraftMovie?> GetByPublicIdAsync(
    string publicId,
    CancellationToken cancellationToken
  ) =>
    _dbContext.GuestDraftMovies.FirstOrDefaultAsync(m => m.PublicId == publicId, cancellationToken);
}
