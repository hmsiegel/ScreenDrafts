namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftPartsRepository : IRepository
{
  Task<DraftPart?> GetByIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken);

  void Update(DraftPart draftPart);
}
