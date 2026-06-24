namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftRepository : IRepository<Draft, DraftId>
{
  Task<Draft?> GetDraftByPublicId(string publicId, CancellationToken cancellationToken);
  Task<Draft?> GetDraftByPublicIdWithPartsAsNoTrackingAsync(
    string publicId,
    CancellationToken cancellationToken
  );
  Task<Draft?> GetDraftByPublicIdWithPartsAsync(
    string publicId,
    CancellationToken cancellationToken
  );
  Task<Draft?> GetDraftByPublicIdForUpdateAsync(
    string publicId,
    CancellationToken cancellationToken
  );

  /// <summary>
  /// Loads a draft with all its parts and their participants.
  /// Used by board fan-out handlers to enumerate unique participants for sync.
  /// </summary>
  Task<Draft?> GetByIdWithPartsAndParticipantsAsync(
    DraftId draftId,
    CancellationToken cancellationToken
  );
}
