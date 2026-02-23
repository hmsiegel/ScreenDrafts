namespace ScreenDrafts.Modules.Reporting.Features.Outbox;

public sealed class ReportingDomainEventDispatcher : IReportingDomainEventDispatcher
{
  public async Task DispatchAsync(IDomainEvent domainEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    var handlers = DomainEventHandlersFactory.GetHandlers(
      domainEvent.GetType(),
      provider,
      typeof(ReportingDomainEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(domainEvent);
    }
  }
}
