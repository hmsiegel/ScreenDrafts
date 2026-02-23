namespace ScreenDrafts.Modules.RealTimeUpdates.Features.Outbox;

public sealed class RealTimeUpdatesDomainEventDispatcher : IRealTimeUpdatesDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(RealTimeUpdatesDomainEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
