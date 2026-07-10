namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class CommissionerOverrideAppliedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  int tmdbId,
  Guid participantId,
  Guid draftId,
  string draftPublicId,
  int participantKind,
  string moviePublicId,
  string movieTitle,
  int boardPosition,
  int playOrder
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int TmdbId { get; init; } = tmdbId;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
  public string MoviePublicId { get; init; } = moviePublicId;
  public string MovieTitle { get; init; } = movieTitle;
  public int BoardPosition { get; init; } = boardPosition;
  public int PlayOrder { get; init; } = playOrder;
}
