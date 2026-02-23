namespace ScreenDrafts.Modules.Integrations.Features.Inbox;

public class IntegrationsIntegrationEventDispatcher : IIntegrationsIntegrationEventDispatcher
{
  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      typeof(IntegrationsIntegrationEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
