namespace ScreenDrafts.Modules.Integrations.Features.Outbox;

public sealed class IntegrationsDomainEventDispatcher : IIntegrationsDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(IntegrationsDomainEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
