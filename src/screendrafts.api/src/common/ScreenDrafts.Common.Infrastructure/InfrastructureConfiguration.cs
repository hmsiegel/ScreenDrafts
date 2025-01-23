namespace ScreenDrafts.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      string databaseConnectionString)
  {
    var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
    services.TryAddSingleton(npgsqlDataSource);

    services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

    return services;
  }
}
