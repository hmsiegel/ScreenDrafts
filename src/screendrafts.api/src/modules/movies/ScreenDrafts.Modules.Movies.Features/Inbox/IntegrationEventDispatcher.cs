namespace ScreenDrafts.Modules.Movies.Features.Inbox;

public class MoviesIntegrationEventDispatcher : IMoviesIntegrationEventDispatcher
{
  public async Task DispatchAsync(IIntegrationEvent integrationEvent, IServiceProvider provider)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var handlers = IntegrationEventHandlersFactory.GetHandlers(
      integrationEvent.GetType(),
      provider,
      typeof(MoviesIntegrationEventDispatcher).Assembly);

    foreach (var handler in handlers)
    {
      await handler.Handle(integrationEvent);
    }
  }
}
