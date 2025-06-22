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
  var builder = Host.CreateDefaultBuilder(args)
      .UseSerilog((context, services, configuration) =>
      {
        configuration
          .ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
          .Enrich.FromLogContext();
      })
      .ConfigureServices((_, services) =>
      {
        services.AddSingleton(configuration);

        // Configure DatabaseSettings for MoviesModule
        services.Configure<DatabaseSettings>(o => 
          o.ConnectionString = configuration.GetConnectionStringOrThrow("Database"));

        services.AddMoviesModule(configuration);

        services.AddRepositoriesFromModules([typeof(MoviesModule).Assembly]);

        services.AddApplication(
        [
          AssemblyReference.Assembly,
          ScreenDrafts.Seeding.Movies.AssemblyReference.Assembly
        ]);

        services.AddSeedingInfrastructure();
        services.AddMovieSeeders();

        services.TryAddScoped<SqlInsertHelper>();

        services.AddOptions<ImdbSettings>()
          .Bind(configuration.GetSection("Imdb"))
          .ValidateDataAnnotations();

        services.AddScoped<IImdbService, ImdbService>();

        services.AddOptions<OmdbSettings>()
          .Bind(configuration.GetSection("Omdb"))
          .ValidateDataAnnotations();

        services.AddScoped<IOmdbService, OmdbService>();
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

