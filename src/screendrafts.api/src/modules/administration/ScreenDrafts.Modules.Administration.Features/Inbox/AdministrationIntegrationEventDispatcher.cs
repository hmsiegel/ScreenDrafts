using Serilog;

namespace ScreenDrafts.Modules.Administration.Features.Inbox;

public class AdministrationIntegrationEventDispatcher : IAdministrationIntegrationEventDispatcher
{
  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    Log.Information("Dispatching integration event {IntegrationEventId} of type {IntegrationEventType}",
      integrationEvent.Id, integrationEvent.GetType().FullName);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      typeof(AdministrationIntegrationEventDispatcher).Assembly)
      .ToList(); // Materialize the collection

    Log.Information("Found {HandlersCount} handlers for integration event {IntegrationEventId}",
      handlers.Count, integrationEvent.Id); // Use .Count property instead of .Count()

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
