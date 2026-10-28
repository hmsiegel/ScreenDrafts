namespace ScreenDrafts.Common.Infrastructure.EventBus;

internal sealed class EventBus(EventBusRuntime runtime) : IEventBus
{
  private readonly EventBusRuntime _runtime = runtime;

  public async Task PublishAsync<T>(
    T integrationEvent,
    CancellationToken cancellationToken = default
  )
    where T : IIntegrationEvent
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);
    cancellationToken.ThrowIfCancellationRequested();

    await _runtime.Publisher.Publish(integrationEvent);
  }
}
