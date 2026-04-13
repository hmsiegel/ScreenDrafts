namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class PickAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string imdbId,
  string movieTitle)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; set; } = draftPartId;
  public string ImdbId { get; set; } = imdbId;
  public string MovieTitle { get; set; } = movieTitle;
}
