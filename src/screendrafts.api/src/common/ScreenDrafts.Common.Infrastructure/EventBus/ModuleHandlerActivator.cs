namespace ScreenDrafts.Common.Infrastructure.EventBus;

/// <summary>
/// Resolves only the handlers one module registered. Rebus runs every handler it finds for a
/// message type, so a shared container would hand each module's bus every module's handlers.
/// </summary>
internal sealed class ModuleHandlerActivator(
  IServiceScopeFactory scopeFactory,
  IReadOnlyList<ConsumerRegistration> registrations
) : IHandlerActivator
{
  private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

  private readonly Dictionary<Type, Type[]> _handlerTypesByEvent = registrations
    .GroupBy(r => r.EventType)
    .ToDictionary(g => g.Key, g => g.Select(r => r.HandlerType).Distinct().ToArray());

  public Task<IEnumerable<IHandleMessages<TMessage>>> GetHandlers<TMessage>(
    TMessage message,
    ITransactionContext transactionContext
  )
  {
    ArgumentNullException.ThrowIfNull(transactionContext);

    if (!_handlerTypesByEvent.TryGetValue(typeof(TMessage), out var handlerTypes))
    {
      return Task.FromResult(Enumerable.Empty<IHandleMessages<TMessage>>());
    }

    var scope = _scopeFactory.CreateScope();
    transactionContext.OnDisposed(_ => scope.Dispose());

    IEnumerable<IHandleMessages<TMessage>> handlers = handlerTypes
      .Select(type =>
        (IHandleMessages<TMessage>)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type)
      )
      .ToList();

    return Task.FromResult(handlers);
  }
}
