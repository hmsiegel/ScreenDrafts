using ScreenDrafts.Common.Application.Messaging.Dispatchers;

namespace ScreenDrafts.Modules.Drafts.Features.Outbox;

public sealed class DraftsDomainEventDispatcher : IDraftsDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(DraftsDomainEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
