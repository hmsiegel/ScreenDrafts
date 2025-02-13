namespace ScreenDrafts.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  private const string MongoDbDiagnosticSource = "MongoDB.Driver.Core.Extensions.DiagnosticSources";

  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      string serviceName,
      Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
      RabbitMqSettings rabbitMqSettings,
      string databaseConnectionString,
      string redisConnectionString,
      string mongoConnectionString)
  {
    services.AddAuthenticationInternal();

    services.AddAuthorizationInternal();

    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

    services.TryAddSingleton<IEventBus, EventBus.EventBus>();

    services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

    var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
    services.TryAddSingleton(npgsqlDataSource);

    services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

    services.TryAddScoped<ICsvFileService, CsvFileService>();

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
                  .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
                  .AddSource(MongoDbDiagnosticSource);

          tracing.AddOtlpExporter();
        });

    var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);

    mongoClientSettings.ClusterConfigurator = c => c.Subscribe(
        new DiagnosticsActivityEventSubscriber(
            new InstrumentationOptions
            {
              CaptureCommandText = true,
            }));

    services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoClientSettings));

    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

    return services;
  }
}
