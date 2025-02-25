namespace ScreenDrafts.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    string serviceName,
    Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
    RabbitMqSettings rabbitMqSettings,
    string databaseConnectionString,
    string redisConnectionString,
    string mongoConnectionString,
    Assembly[] infrastructureAssemblies)
  {
    services.AddAuthenticationInternal();

    services.AddAuthorizationInternal();

    services.AddRepositoriesFromModules(infrastructureAssemblies);

    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

    services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

    var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
    services.TryAddSingleton(npgsqlDataSource);

    services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

    services.TryAddScoped<ICsvFileService, CsvFileService>();

    SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

    services.AddQuartzService();

    services.AddCaching(redisConnectionString);

    services.AddEventBus(serviceName, moduleConfigureConsumers, rabbitMqSettings);

    services.ConfigureOpenTelemetry(serviceName);

    services.AddMongoDb(mongoConnectionString);

    return services;
  }

  private static IServiceCollection AddRepositoriesFromModules(this IServiceCollection services, Assembly[] infrastructureAssemblies)
  {
    services.Scan(scan => scan
        .FromAssemblies(infrastructureAssemblies)
        .AddClasses(classes => classes.AssignableTo<IRepository>(), false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    return services;
  }
}
