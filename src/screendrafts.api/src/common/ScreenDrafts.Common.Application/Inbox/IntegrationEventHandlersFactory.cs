namespace ScreenDrafts.Common.Application.Inbox;

public static class IntegrationEventHandlersFactory
{
  private static readonly ConcurrentDictionary<string, Type[]> _handlersDictionary = new();

  public static IEnumerable<IIntegrationEventHandler> GetHandlers(
      Type type,
      IServiceProvider serviceProvider,
      Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(type);
    ArgumentNullException.ThrowIfNull(assembly);

    Type[] integrationEventHandlerTypes = _handlersDictionary.GetOrAdd(
        $"{assembly.GetName().Name}{type.Name}",
        _ =>
        {
          var integrationEventHandlerTypes = assembly.GetTypes()
                  .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler<>).MakeGenericType(type)))
                  .ToArray();

          return integrationEventHandlerTypes;
        });

    List<IIntegrationEventHandler> handlers = [];
    foreach (var integrationEventHandlerType in integrationEventHandlerTypes)
    {
      var integrationEventHandler = serviceProvider.GetRequiredService(integrationEventHandlerType);

      handlers.Add((integrationEventHandler as IIntegrationEventHandler)!);
    }

    return handlers;
  }
}
