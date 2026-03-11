using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Movies.IntegrationEvents;

public sealed class FetchMovieRequestedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  int tmdbId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public int TmdbId { get; init; } = tmdbId;
}
