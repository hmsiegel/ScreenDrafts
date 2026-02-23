namespace ScreenDrafts.Modules.Administration.Features.Outbox;

public sealed class AdministrationDomainEventDispatcher : IAdministrationDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(AdministrationDomainEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
