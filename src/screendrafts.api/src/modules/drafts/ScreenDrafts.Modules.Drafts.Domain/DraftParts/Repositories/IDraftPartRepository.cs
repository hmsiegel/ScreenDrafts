namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;

public interface IDraftPartRepository : IRepository
{
  Task<DraftPart?> GetByIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken);
  Task<DraftPart?> GetByPublicIdAsync(string draftPartId, CancellationToken cancellationToken);
  Task<DraftPart?> GetByPublicIdWithSubDraftsAsync(string draftPartId, CancellationToken cancellationToken);
  Task<DraftPart?> GetByZoomSessionNameAsync(string zoomSessionName, CancellationToken cancellationToken);
  void Update(DraftPart draftPart);
}
