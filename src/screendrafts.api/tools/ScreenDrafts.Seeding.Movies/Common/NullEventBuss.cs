using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Seeding.Movies.Common;

internal sealed class NullEventBus : IEventBus
{
  public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : IIntegrationEvent
  {
    return Task.CompletedTask;
  }
}
