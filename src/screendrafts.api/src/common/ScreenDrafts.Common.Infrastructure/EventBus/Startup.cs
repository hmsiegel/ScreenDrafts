namespace ScreenDrafts.Common.Infrastructure.EventBus;

internal static class Startup
{
  internal static IServiceCollection AddEventBus(
    this IServiceCollection services,
    string serviceName,
    Action<IModuleConsumerRegistry, string>[] moduleConfigureConsumers,
    RabbitMqSettings rabbitMqSettings
  )
  {
    ArgumentNullException.ThrowIfNull(serviceName);
    ArgumentNullException.ThrowIfNull(moduleConfigureConsumers);

    string instanceid = serviceName
      .ToUpperInvariant()
      .Replace(" ", "-", StringComparison.InvariantCultureIgnoreCase);

    var registry = new ModuleConsumerRegistry();

    foreach (var configureConsumer in moduleConfigureConsumers)
    {
      configureConsumer(registry, instanceid);
    }

    services.AddSingleton(registry);
    services.AddSingleton(rabbitMqSettings);
    services.AddSingleton<EventBusRuntime>();
    services.TryAddSingleton<IEventBus, EventBus>();
    services.AddHostedService<EventBusHostedService>();

    return services;
  }
}
