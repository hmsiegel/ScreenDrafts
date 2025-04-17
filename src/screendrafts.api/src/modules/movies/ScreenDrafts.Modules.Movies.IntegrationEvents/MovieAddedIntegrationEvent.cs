namespace ScreenDrafts.Modules.Movies.IntegrationEvents;

public sealed class MovieAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid movieId,
  string title,
  string imdbId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid MovieId { get; init; } = movieId;

  public string Title { get; init; } = title;

  public string ImdbId { get; init; } = imdbId;
}
