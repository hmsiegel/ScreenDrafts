namespace ScreenDrafts.Common.Infrastructure.EventBus;

/// <summary>
/// Starts one publish-only Rebus bus plus one consuming bus per module queue, and subscribes
/// each consuming bus to the event types its module registered.
/// </summary>
internal sealed class EventBusHostedService(
  ModuleConsumerRegistry registry,
  EventBusRuntime runtime,
  IServiceScopeFactory scopeFactory,
  ILoggerFactory loggerFactory,
  RabbitMqSettings settings
) : IHostedService
{
  private readonly List<IBus> _buses = [];

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var connectionString = BuildConnectionString(settings);

    var publisher = Configure
      .OneWayClient()
      .Logging(l => l.MicrosoftExtensionsLogging(loggerFactory))
      .Transport(t => t.UseRabbitMqAsOneWayClient(connectionString))
      .Serialization(s => s.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
      .Start();

    _buses.Add(publisher);
    runtime.SetPublisher(publisher);

    foreach (var (queueName, registrations) in registry.Queues)
    {
      await StartConsumerBusAsync(connectionString, queueName, registrations);
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    foreach (var bus in _buses)
    {
      bus.Dispose();
    }

    _buses.Clear();
    return Task.CompletedTask;
  }

  private async Task StartConsumerBusAsync(
    string connectionString,
    string queueName,
    IReadOnlyList<ConsumerRegistration> registrations
  )
  {
    var bus = Configure
      .With(new ModuleHandlerActivator(scopeFactory, registrations))
      .Logging(l => l.MicrosoftExtensionsLogging(loggerFactory))
      .Transport(t => t.UseRabbitMq(connectionString, queueName))
      .Serialization(s => s.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
      .Start();

    _buses.Add(bus);

    foreach (var eventType in registrations.Select(r => r.EventType).Distinct())
    {
      await bus.Subscribe(eventType);
    }
  }

  private static string BuildConnectionString(RabbitMqSettings rabbitMq)
  {
    var builder = new UriBuilder(rabbitMq.Host)
    {
      UserName = Uri.EscapeDataString(rabbitMq.UserName),
      Password = Uri.EscapeDataString(rabbitMq.Password),
    };

    return builder.Uri.AbsoluteUri;
  }
}
