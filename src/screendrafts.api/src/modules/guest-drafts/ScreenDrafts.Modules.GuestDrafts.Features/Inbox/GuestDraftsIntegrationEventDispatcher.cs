namespace ScreenDrafts.Modules.GuestDrafts.Features.Inbox;

public class GuestDraftsIntegrationEventDispatcher : IGuestDraftsIntegrationEventDispatcher
{
  private const string ModuleName = "GuestDrafts";

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
