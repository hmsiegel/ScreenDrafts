namespace ScreenDrafts.Common.Infrastructure.Database;

public static class DbContextOptionsBuilderExtensions
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Reviewed")]
  public static DbContextOptionsBuilder UseModuleDefaults(
    this DbContextOptionsBuilder optionsBuilder,
    string moduleName,
    string schema,
    IServiceProvider sp)
  {
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionStringOrThrow(moduleName);

    return optionsBuilder.UseNpgsql(
      connectionString,
      npgsql =>
      npgsql.MigrationsHistoryTable(
        HistoryRepository.DefaultTableName,
        schema.ToLowerInvariant()))
      .UseSnakeCaseNamingConvention()
      .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
  }
}
