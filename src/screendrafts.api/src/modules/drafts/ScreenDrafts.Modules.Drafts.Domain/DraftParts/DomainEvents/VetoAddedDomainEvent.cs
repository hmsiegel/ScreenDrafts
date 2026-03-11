namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class VetoAddedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  int? tmdbId,
  Guid participantId,
  int participantKind) : DomainEvent

{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int? TmdbId { get; init; } = tmdbId;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
}
