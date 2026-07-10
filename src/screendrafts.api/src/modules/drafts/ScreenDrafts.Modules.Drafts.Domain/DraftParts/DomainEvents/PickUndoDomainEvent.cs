namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class PickUndoDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  int playOrder,
  int boardPosition,
  int tmdbId,
  string movieTitle,
  Guid draftId,
  string draftPublicId,
  string moviePublicId,
  Guid playedByParticipantId,
  int playedByParticipantKind
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;
  public int TmdbId { get; init; } = tmdbId;
  public string MovieTitle { get; init; } = movieTitle;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public string MoviePublicId { get; init; } = moviePublicId;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
  public int PlayedByParticipantKind { get; init; } = playedByParticipantKind;
}
