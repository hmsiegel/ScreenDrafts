namespace ScreenDrafts.Common.Infrastructure.EventBus;

internal static class Startup
{
  internal static IServiceCollection AddEventBus(
    this IServiceCollection services,
    string serviceName,
    Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
    RabbitMqSettings rabbitMqSettings)
  {
    services.TryAddSingleton<IEventBus, EventBus>();

    services.AddMassTransit(config =>
    {
      string instanceid = serviceName.ToUpperInvariant()
        .Replace(" ", "-", StringComparison.InvariantCultureIgnoreCase);

      foreach (Action<IRegistrationConfigurator, string> configureConsumer in moduleConfigureConsumers)
      {
        configureConsumer(config, instanceid);
      }

      config.SetKebabCaseEndpointNameFormatter();

      config.UsingRabbitMq((context, cfg) =>
          {
            cfg.Host(new Uri(rabbitMqSettings.Host), h =>
                {
                  h.Username(rabbitMqSettings.UserName);
                  h.Password(rabbitMqSettings.Password);
                });
            cfg.ConfigureEndpoints(context);
          });
    });

    return services;
  }
}
