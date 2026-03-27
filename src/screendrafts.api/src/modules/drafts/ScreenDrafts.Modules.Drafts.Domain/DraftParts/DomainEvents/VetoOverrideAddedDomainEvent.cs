namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class VetoOverrideAddedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  int tmdbId,
  Guid participantId,
  int participantKind,
  Guid draftId,
  string draftPublicId,
  int canonicalPolicyValue) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int TmdbId { get; init; } = tmdbId;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
  public int CanonicalPolicyValue { get; init; } = canonicalPolicyValue;
}
