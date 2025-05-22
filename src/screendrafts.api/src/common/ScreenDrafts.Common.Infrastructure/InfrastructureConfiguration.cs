namespace ScreenDrafts.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration,
    string serviceName,
    Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
    RabbitMqSettings rabbitMqSettings,
    string redisConnectionString,
    string mongoConnectionString,
    Assembly[] infrastructureAssemblies)
  {
    services.AddCorsPolicy(configuration);

    services.AddAuthenticationInternal();

    services.AddAuthorizationInternal();

    services.AddCoreInfrastructure();

    services.AddRepositoriesFromModules(infrastructureAssemblies);

    services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

    SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

    services.AddCaching(redisConnectionString);

    services.ConfigureOpenTelemetry(serviceName);

    services.AddMongoDb(mongoConnectionString);

    services.AddEventBus(serviceName, moduleConfigureConsumers, rabbitMqSettings);

    return services;
  }

  public static IServiceCollection AddSeedingInfrastructure(
    this IServiceCollection services)
  {
    services.AddCoreInfrastructure();
    return services;
  }

  private static IServiceCollection AddCoreInfrastructure(this IServiceCollection services)
  {
    // Add core infrastructure services here
    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

    services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

    services.TryAddScoped<ICsvFileService, CsvFileService>();

    services.AddQuartzService();

    return services;
  }


  private static IServiceCollection AddRepositoriesFromModules(
    this IServiceCollection services,
    Assembly[] infrastructureAssemblies)
  {
    services.Scan(scan => scan
        .FromAssemblies(infrastructureAssemblies)
        .AddClasses(classes => classes.AssignableTo<IRepository>(), false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    return services;
  }
}
