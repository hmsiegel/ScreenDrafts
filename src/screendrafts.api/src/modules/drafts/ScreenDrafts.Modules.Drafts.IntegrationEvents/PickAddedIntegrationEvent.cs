namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class PickAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  string imdbId,
  string movieTitle,
  int? tmdbId,
  int boardPosition,
  int playOrder,
  Guid participantId,
  int participantKind
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public string ImdbId { get; init; } = imdbId;
  public string MovieTitle { get; init; } = movieTitle;
  public int? TmdbId { get; init; } = tmdbId;
  public int BoardPosition { get; init; } = boardPosition;
  public int PlayOrder { get; init; } = playOrder;
  public Guid ParticipantId { get; init; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
}
