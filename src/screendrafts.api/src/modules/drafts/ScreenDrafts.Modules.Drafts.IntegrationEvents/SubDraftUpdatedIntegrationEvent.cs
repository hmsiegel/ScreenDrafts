namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class SubDraftUpdatedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  string subDraftPublicId,
  int status,
  int? subjectKind,
  string? subjectName,
  string? subjectImdbId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public string SubDraftPublicId { get; init; } = subDraftPublicId;
  public int Status { get; init; } = status;
  public int? SubjectKind { get; init; } = subjectKind;
  public string? SubjectName { get; init; } = subjectName;
  public string? SubjectImdbId { get; init; } = subjectImdbId;
}
