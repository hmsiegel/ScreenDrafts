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
    .CreateLogger();

var connectionString = configuration.GetConnectionStringOrThrow("Database");

try
{
  var builder = Host.CreateDefaultBuilder(args)
      .UseDefaultServiceProvider(opt =>
      {
        opt.ValidateScopes = false;
        opt.ValidateOnBuild = false;
      })
      .UseSerilog((context, services, configuration) =>
      {
        configuration
          .ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
          .Enrich.FromLogContext();
      })
      .ConfigureServices((hostContext, services) =>
      {
        services.AddSingleton(configuration);
        services.AddSeedingInfrastructure(connectionString);
        services.AddHonorificsSeeders();
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
