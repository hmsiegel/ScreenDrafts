namespace ScreenDrafts.Common.Infrastructure.EventBus;

internal sealed class ModuleConsumerRegistry : IModuleConsumerRegistry
{
  private readonly Dictionary<string, List<ConsumerRegistration>> _queues = new(
    StringComparer.Ordinal
  );
  private string? _currentQueue;

  public IReadOnlyDictionary<string, List<ConsumerRegistration>> Queues => _queues;

  public void ForQueue(string queueName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

    _currentQueue = queueName;
    _queues.TryAdd(queueName, []);
  }

  public void AddConsumer<THandler>()
    where THandler : class
  {
    var eventTypes = typeof(THandler)
      .GetInterfaces()
      .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
      .Select(i => i.GetGenericArguments()[0])
      .ToList();

    if (eventTypes.Count == 0)
    {
      throw new InvalidOperationException(
        $"{typeof(THandler).Name} does not implement IHandleMessages<T>."
      );
    }

    foreach (var eventType in eventTypes)
    {
      AddConsumer(eventType, typeof(THandler));
    }
  }

  public void AddConsumer(Type eventType, Type handlerType)
  {
    ArgumentNullException.ThrowIfNull(eventType);
    ArgumentNullException.ThrowIfNull(handlerType);

    if (_currentQueue is null)
    {
      throw new InvalidOperationException("Call ForQueue before adding consumers.");
    }

    _queues[_currentQueue].Add(new ConsumerRegistration(eventType, handlerType));
  }
}
