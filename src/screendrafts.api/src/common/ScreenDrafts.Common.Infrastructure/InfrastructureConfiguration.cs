namespace ScreenDrafts.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      Action<IRegistrationConfigurator>[] moduleConfigureConsumers,
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
      foreach (var configureConsumer in moduleConfigureConsumers)
      {
        configureConsumer(config);
      }

      config.SetKebabCaseEndpointNameFormatter();

      config.UsingInMemory((context, cfg) =>
      {
        cfg.ConfigureEndpoints(context);
      });
    });

    return services;
  }
}
