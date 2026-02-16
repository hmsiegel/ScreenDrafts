var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

var config = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
  var bootstrapConnectionString = config.GetConnectionStringOrThrow("Database");

  Log.Information("=== Bootstrapping Schemas ===");

  var bootstrapResult = DeployChanges.To
    .PostgresqlDatabase(bootstrapConnectionString)
    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly,
    s => s.EndsWith("0001_create_schema.sql", StringComparison.OrdinalIgnoreCase))
    .WithTransaction()
    .JournalToPostgresqlTable("public", "schema_versions_bootstrap")
    .LogToConsole()
    .Build()
    .PerformUpgrade();

  if (!bootstrapResult.Successful)
  {
    Log.Error(bootstrapResult.Error, "Bootstrap migration failed.");
    return 1;
  }

  var modules = new[]
  {
    ("Administration", "Administration", "administration"),
    ("Audit", "Audit", "audit"),
    ("Communications", "Communications", "communications"),
    ("Drafts", "Drafts", "drafts"),
    ("Integrations", "Integrations", "integrations"),
    ("Movies", "Movies", "movies"),
    ("RealTimeUpdates", "RealTimeUpdates", "real_time_updates"),
    ("Reporting", "Reporting", "reporting"),
    ("Users", "Users", "users")
  };

  foreach (var (module, connectionKey, schema) in modules)
  {
    var connectionString = config.GetConnectionStringOrThrow(connectionKey);

    Log.Information("=== Migrating {Module} ===", module);

    // Run the EF baseline script with no transaction wrapping.
    var efResult = DeployChanges.To
      .PostgresqlDatabase(connectionString)
      .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly,
      s => s.StartsWith($"ScreenDrafts.Tools.DbMigrator.Scripts.{module}.", StringComparison.OrdinalIgnoreCase)
                && s.Contains("_ef_", StringComparison.OrdinalIgnoreCase))
      .WithoutTransaction()
      .WithVariablesDisabled()
      .JournalToPostgresqlTable(schema, "schema_versions")
      .LogToConsole()
      .Build()
      .PerformUpgrade();

    if (!efResult.Successful)
    {
      Log.Error(efResult.Error, "{Module} baseline migration failed.", module);
      return 1;
    }

    var sqlResult = DeployChanges.To
      .PostgresqlDatabase(connectionString)
      .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly,
      s => s.StartsWith($"ScreenDrafts.Tools.DbMigrator.Scripts.{module}.", StringComparison.OrdinalIgnoreCase)
              && !s.Contains("_ef_", StringComparison.OrdinalIgnoreCase)
              && !s.EndsWith("0001_create_schema.sql", StringComparison.OrdinalIgnoreCase))
      .WithTransaction()
      .WithVariablesDisabled()
      .JournalToPostgresqlTable(schema, "schema_versions")
      .LogToConsole()
      .Build()
      .PerformUpgrade();

    if (!sqlResult.Successful)
    {
      Log.Error(sqlResult.Error, "{Module} migration failed.", module);
      return 1;
    }

    Log.Information("{Module} migration successful.", module);
  }
  Log.Information("All modules migrated successfully.");
  return 0;
}
catch (ScreenDraftsException ex)
{
  Log.Fatal(ex, "Migrator terminated unexpectedly.");
  return 1;
}
finally
{
  await Log.CloseAndFlushAsync();

}

