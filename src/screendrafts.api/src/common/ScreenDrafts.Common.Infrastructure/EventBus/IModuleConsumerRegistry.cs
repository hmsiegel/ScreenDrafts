namespace ScreenDrafts.Common.Infrastructure.EventBus;

/// <summary>
/// Collects the integration event handlers each module wants delivered. Every queue gets its own
/// Rebus bus, so one module's handler failing never retries another module's inbox write.
/// </summary>
public interface IModuleConsumerRegistry
{
  /// <summary>Starts a queue; later <c>AddConsumer</c> calls attach to it.</summary>
  void ForQueue(string queueName);

  /// <summary>Registers <typeparamref name="THandler"/> for every event type it handles.</summary>
  void AddConsumer<THandler>()
    where THandler : class;

  /// <summary>Registers <paramref name="handlerType"/> for <paramref name="eventType"/>.</summary>
  void AddConsumer(Type eventType, Type handlerType);
}
