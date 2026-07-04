namespace ScreenDrafts.Modules.GuestDrafts.Features.Outbox;

public sealed class GuestDraftsDomainEventDispatcher : IGuestDraftsDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(GuestDraftsDomainEventDispatcher).Assembly
    );

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
