namespace ScreenDrafts.Modules.Audit.Features.Outbox;

public sealed class AuditDomainEventDispatcher : IAuditDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(AuditDomainEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
