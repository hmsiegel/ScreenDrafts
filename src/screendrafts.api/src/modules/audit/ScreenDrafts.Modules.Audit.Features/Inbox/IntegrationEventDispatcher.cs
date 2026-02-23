namespace ScreenDrafts.Modules.Audit.Features.Inbox;

public class AuditIntegrationEventDispatcher : IAuditIntegrationEventDispatcher
{
  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      typeof(AuditIntegrationEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
