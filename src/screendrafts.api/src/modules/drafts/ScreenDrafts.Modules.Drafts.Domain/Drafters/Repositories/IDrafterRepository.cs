namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;

public interface IDrafterRepository : IRepository<Drafter, DrafterId>
{
  Task<bool> ExistsForPersonAsync(string personPublicId, CancellationToken cancellationToken);
}
