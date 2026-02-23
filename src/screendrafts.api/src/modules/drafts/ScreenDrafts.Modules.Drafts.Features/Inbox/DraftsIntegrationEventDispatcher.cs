using Serilog;

namespace ScreenDrafts.Modules.Drafts.Features.Inbox;

public class DraftsIntegrationEventDispatcher : IAdministrationIntegrationEventDispatcher
{
  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      AssemblyReference.Assembly);

    Log.Information("Dispatching integration event {IntegrationEventType}", integrationEvent.GetType().Name);

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
