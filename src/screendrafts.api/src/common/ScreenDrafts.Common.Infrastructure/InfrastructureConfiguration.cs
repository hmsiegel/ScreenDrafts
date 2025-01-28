namespace ScreenDrafts.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      string databaseConnectionString,
      string redisConnectionString)
  {
    var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
    services.TryAddSingleton(npgsqlDataSource);

    services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

    services.TryAddSingleton<PublishDomainEventsInterceptor>();

    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

    IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString, opt =>
    {
      opt.AbortOnConnectFail = false;
    });

    services.TryAddSingleton(connectionMultiplexer);

    services.AddStackExchangeRedisCache(opt =>
      opt.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));

    services.AddScoped<ICacheService, CacheService>();

    return services;
  }
}
