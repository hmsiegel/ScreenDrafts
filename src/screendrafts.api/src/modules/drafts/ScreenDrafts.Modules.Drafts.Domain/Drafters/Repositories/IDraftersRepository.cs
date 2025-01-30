
namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;

public interface IDraftersRepository
{
  void Add(Drafter drafter);
  Task<Drafter?> GetByIdAsync(Guid drafterId, CancellationToken cancellationToken);
}
