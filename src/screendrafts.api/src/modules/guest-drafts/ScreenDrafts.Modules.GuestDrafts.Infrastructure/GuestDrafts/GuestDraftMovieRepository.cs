using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftMovieRepository(GuestDraftsDbContext dbContext) : IMovieRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(Movie movie) => _dbContext.GuestDraftMovies.Add(movie);

  public Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    _dbContext.GuestDraftMovies.AnyAsync(m => m.PublicId == publicId, cancellationToken);

  public Task<Movie?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    _dbContext.GuestDraftMovies.FirstOrDefaultAsync(m => m.PublicId == publicId, cancellationToken);
}
