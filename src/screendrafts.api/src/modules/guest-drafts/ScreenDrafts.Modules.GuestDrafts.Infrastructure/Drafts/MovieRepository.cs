namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Drafts;

internal sealed class MovieRepository(GuestDraftsDbContext dbContext) : IMovieRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(Movie movie) => _dbContext.Movies.Add(movie);

  public Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    _dbContext.Movies.AnyAsync(m => m.PublicId == publicId, cancellationToken);

  public Task<Movie?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    _dbContext.Movies.FirstOrDefaultAsync(m => m.PublicId == publicId, cancellationToken);
}
