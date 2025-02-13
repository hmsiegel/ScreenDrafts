namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;

public interface IDraftersRepository
{
  void Add(Drafter drafter);
  Task<Drafter?> GetByIdAsync(DrafterId drafterId, CancellationToken cancellationToken);

  Task<List<Drafter>> GetAll(CancellationToken cancellationToken = default);
}
