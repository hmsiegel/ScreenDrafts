namespace ScreenDrafts.Web.Extensions;

internal static class ConnectionStringExtensions
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Vulnerability", "S2068:Credentials should not be hard-coded", Justification = "Dummy credentials for design-time only")]
  private const string Dummy = "Host=localhost;Port=5432;Database=dummy;Username=postgres;Password=postgres";

  public static string AddPostgresDatabase(
    this IServiceCollection services,
    IConfiguration configuration,
    Action<NpgsqlDataSourceBuilder>? builderConfig = null)
  {
    var isDesignTime = AppDomain.CurrentDomain.FriendlyName.StartsWith("NSwag", StringComparison.OrdinalIgnoreCase)
      || Environment.GetEnvironmentVariable("NSWAG_SKIP_DB") == "true";

    var connectionString = isDesignTime
      ? Dummy
      : configuration.GetConnectionStringOrThrow("Database");

    // Fix: Use an object initializer to set the init-only property
    services.Configure<DatabaseSettings>(o => o.ConnectionString = connectionString);

    if (!isDesignTime)
    {
      var npgsqlDataSource = new NpgsqlDataSourceBuilder(connectionString);
      builderConfig?.Invoke(npgsqlDataSource);
      services.AddSingleton(npgsqlDataSource.Build());
    }

    return connectionString;
  }
}
