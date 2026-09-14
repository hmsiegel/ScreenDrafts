using Serilog;

namespace ScreenDrafts.Modules.Drafts.Features.Inbox;

public class DraftsIntegrationEventDispatcher : IDraftsIntegrationEventDispatcher
{
  private const string ModuleName = "Drafts";

  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      AssemblyReference.Assembly
    );

    Log.Information(
      "Dispatching integration event {IntegrationEventType} in {ModuleName}",
      integrationEvent.GetType().Name,
      ModuleName
    );

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
