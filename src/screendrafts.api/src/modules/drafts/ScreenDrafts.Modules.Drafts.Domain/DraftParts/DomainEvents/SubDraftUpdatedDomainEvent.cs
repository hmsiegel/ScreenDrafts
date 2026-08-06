namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

/// <summary>
/// Raised whenever a sub-draft's summary-level state changes outside of a
/// pick/veto — trivia resolution (Pending → Active, subject revealed) and
/// position choice (winner assigned). Both moments need to reach the OTHER
/// participant's client, not just the actor's own browser, which local
/// refetch()/callback calls don't cover.
/// </summary>
public sealed class SubDraftUpdatedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  Guid subDraftId,
  string subDraftPublicId,
  int status,
  int? subjectKind,
  string? subjectName,
  string? subjectImdbId
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid SubDraftId { get; init; } = subDraftId;
  public string SubDraftPublicId { get; init; } = subDraftPublicId;
  public int Status { get; init; } = status;
  public int? SubjectKind { get; init; } = subjectKind;
  public string? SubjectName { get; init; } = subjectName;
  public string? SubjectImdbId { get; init; } = subjectImdbId;
}
