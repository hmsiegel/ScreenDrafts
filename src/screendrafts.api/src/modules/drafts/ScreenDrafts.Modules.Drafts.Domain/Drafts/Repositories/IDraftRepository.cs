namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftRepository : IRepository<Draft, DraftId>
{
  void AddCommissionerOverride(CommissionerOverride commissionerOverride);
  Task<Draft?> GetDraftWithDetailsAsync(DraftId draftId, CancellationToken cancellationToken);
  Task<Draft?> GetPreviousDraftAsync(int? episodeNumber, CancellationToken cancellationToken);
  Task<Draft?> GetNextDraftAsync(int? episodeNumber, CancellationToken cancellationToken);
  Task<Draft?> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken);
  Task<DraftPart?> GetDraftPartByIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken);
  Task<List<DraftPart>> GetDraftPartsByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByDraftPartId(DraftPartId draftPartId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByPublicId(string publicId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByPublicIdWithPartsAsNoTrackingAsync(string publicId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByPublicIdWithPartsAsync(string publicId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByPublicIdForUpdateAsync(string publicId, CancellationToken cancellationToken);
}
