namespace ScreenDrafts.Common.Application.Messaging;

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
  Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler
{
  Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
