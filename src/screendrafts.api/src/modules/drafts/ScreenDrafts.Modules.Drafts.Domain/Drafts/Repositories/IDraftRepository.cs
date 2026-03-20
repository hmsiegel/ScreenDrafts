namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftRepository : IRepository<Draft, DraftId>
{
  Task<Draft?> GetDraftByPublicId(string publicId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByPublicIdWithPartsAsNoTrackingAsync(string publicId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByPublicIdWithPartsAsync(string publicId, CancellationToken cancellationToken);
}
