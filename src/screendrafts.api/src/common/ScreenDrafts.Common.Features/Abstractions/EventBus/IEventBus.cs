namespace ScreenDrafts.Common.Features.Abstractions.EventBus;

public interface IEventBus
{
  Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : IIntegrationEvent;
}
