namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Repositories;

public interface IGuestDraftMovieRepository : IRepository
{
  void Add(GuestDraftMovie movie);

  Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken);

  Task<GuestDraftMovie?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken);
}
