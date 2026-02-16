namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;

public interface IDraftPartRepository : IRepository
{
  Task<DraftPart?> GetByIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken);

  void Update(DraftPart draftPart);
}
