namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;

public interface IMovieRepository : IRepository
{
  void Add(Movie movie);

  Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken);

  Task<Movie?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);
}
