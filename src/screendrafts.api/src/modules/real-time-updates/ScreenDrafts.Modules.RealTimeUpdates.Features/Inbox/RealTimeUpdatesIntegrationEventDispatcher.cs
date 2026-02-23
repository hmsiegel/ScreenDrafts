namespace ScreenDrafts.Modules.RealTimeUpdates.Features.Inbox;

public class RealTimeUpdatesIntegrationEventDispatcher : IRealTimeUpdatesIntegrationEventDispatcher
{
  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      typeof(RealTimeUpdatesIntegrationEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
