using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Movies.IntegrationEvents;

public sealed class FetchMovieRequestedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string imdbId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public string ImdbId { get; init; } = imdbId;
}
