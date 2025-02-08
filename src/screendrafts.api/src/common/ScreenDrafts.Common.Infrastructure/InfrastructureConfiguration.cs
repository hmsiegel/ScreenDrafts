using ScreenDrafts.Common.Infrastructure.EventBus;

namespace ScreenDrafts.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      string serviceName,
      Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
      RabbitMqSettings rabbitMqSettings,
      string databaseConnectionString,
      string redisConnectionString)
  {
    services.AddAuthenticationInternal();

    services.AddAuthorizationInternal();

    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

    services.TryAddSingleton<IEventBus, EventBus.EventBus>();

    services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

    var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
    services.TryAddSingleton(npgsqlDataSource);

    services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

    SqlMapper.AddTypeHandler(new GeneciArrayHandler<string>());

    services.AddQuartz();

    services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

    IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString, opt =>
    {
      opt.AbortOnConnectFail = false;
    });

    services.TryAddSingleton(connectionMultiplexer);

    services.AddStackExchangeRedisCache(opt =>
      opt.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));

    services.TryAddSingleton<ICacheService, CacheService>();

    services.AddMassTransit(config =>
    {
      string instanceId = serviceName.ToUpperInvariant().Replace(" ", "-", StringComparison.InvariantCultureIgnoreCase);
      foreach (Action<IRegistrationConfigurator, string> configureConsumer in moduleConfigureConsumers)
      {
        configureConsumer(config, instanceId);
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

    services
      .AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(serviceName))
      .WithTracing(tracing =>
      {
        tracing
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation()
          .AddRedisInstrumentation()
          .AddNpgsql()
          .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);

        tracing.AddOtlpExporter();
      });

    return services;
  }
}
