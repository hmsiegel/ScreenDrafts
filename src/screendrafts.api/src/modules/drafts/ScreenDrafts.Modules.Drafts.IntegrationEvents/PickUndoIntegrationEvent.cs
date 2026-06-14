namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class PickUndoneIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  int playOrder,
  int boardPosition,
  int tmdbId,
  string movieTitle
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;
  public int TmdbId { get; init; } = tmdbId;
  public string MovieTitle { get; init; } = movieTitle;
}
