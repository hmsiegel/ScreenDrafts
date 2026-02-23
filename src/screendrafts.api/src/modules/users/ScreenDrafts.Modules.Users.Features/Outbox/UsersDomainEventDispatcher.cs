namespace ScreenDrafts.Modules.Users.Features.Outbox;

public sealed class UsersDomainEventDispatcher : IUsersDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(UsersDomainEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
