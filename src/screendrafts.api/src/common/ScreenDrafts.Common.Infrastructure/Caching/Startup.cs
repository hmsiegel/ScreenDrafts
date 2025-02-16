namespace ScreenDrafts.Common.Infrastructure.Caching;

internal static class Startup
{
  internal static IServiceCollection AddCaching(this IServiceCollection services, string redisConnectionString)
  {
    IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString, opt =>
    {
      opt.AbortOnConnectFail = false;
    });

    services.TryAddSingleton(connectionMultiplexer);

    services.AddStackExchangeRedisCache(opt =>
        opt.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));

    services.TryAddSingleton<ICacheService, CacheService>();

    return services;
  }
}
