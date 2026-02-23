namespace ScreenDrafts.Modules.Reporting.Features.Inbox;

public class ReportingIntegrationEventDispatcher : IReportingIntegrationEventDispatcher
{
  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      typeof(ReportingIntegrationEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
