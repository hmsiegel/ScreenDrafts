namespace ScreenDrafts.Common.Application.Messaging;

public interface IDomainEventDispatcher
{
  Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider);
}
