namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class VetoAddedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  int? tmdbId,
  Guid participantId,
  int participantKind,
  Guid draftId,
  string draftPublicId,
  int playOrder,
  string? movieTitle,
  Guid playedByParticipantId,
  int playedByParticipantKind
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int? TmdbId { get; init; } = tmdbId;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
  public int PlayOrder { get; init; } = playOrder;
  public string? MovieTitle { get; init; } = movieTitle;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
  public int PlayedByParticipantKind { get; init; } = playedByParticipantKind;
}
