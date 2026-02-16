namespace ScreenDrafts.Common.Application.EventBus;

public interface IIntegrationEventDispatcher
{
  Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider);
}
