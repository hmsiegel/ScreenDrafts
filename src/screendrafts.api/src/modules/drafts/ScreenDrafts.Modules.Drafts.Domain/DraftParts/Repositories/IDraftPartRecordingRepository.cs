namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;

public interface IDraftPartRecordingRepository : IRepository<DraftPartRecording>
{
  Task<DraftPartRecording?> GetByZoomFileIdAsync(string zoomFileId, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<DraftPartRecording>> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default);
  Task<int> CountByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default);
}
