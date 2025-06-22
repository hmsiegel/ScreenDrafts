using Host = Microsoft.Extensions.Hosting.Host;

var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

var configuration = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
  Log.Information("Starting seeding application...");

  var builder = Host.CreateDefaultBuilder(args) // Ensure the correct namespace is used
      .ConfigureServices((hostContext, services) =>
      {
        var configuration = hostContext.Configuration;
        
        // Configure DatabaseSettings for DraftsModule
        services.Configure<DatabaseSettings>(o => 
          o.ConnectionString = configuration.GetConnectionStringOrThrow("Database"));
        
        services.AddDraftsModule(configuration);
        services.AddSeedingInfrastructure();
        services.AddDraftSeeders();
        services.TryAddScoped<SqlInsertHelper>();
        services.AddLogging(builder =>
          {
            builder.AddConsole();
            builder.AddDebug();
          });
      });

  var app = builder.Build();

  using var scope = app.Services.CreateScope();
  var seederExecutor = scope.ServiceProvider.GetRequiredService<SeederExecutor>();

  var selectedModules = SeedingHelper.ParseModules(args);

  await seederExecutor.ExecuteAsync(selectedModules);

  Log.Information("Seeding completed successfully.");
}
catch (ScreenDraftsException ex)
{
  Log.Fatal(ex, "An error occurred during seeding.");
}
finally
{
  await Log.CloseAndFlushAsync();
}

