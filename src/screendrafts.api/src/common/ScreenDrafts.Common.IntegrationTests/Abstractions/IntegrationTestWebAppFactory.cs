namespace ScreenDrafts.Common.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private PostgreSqlContainer? _dbContainer;
  private RedisContainer? _redisContainer;
  private RabbitMqContainer? _rabbitMqContainer;
  private MongoDbContainer? _mongoDbContainer;

  private bool _disposed;
  private bool _containersInitialized;

  public IntegrationTestWebAppFactory()
  {
    InitializeAndStartContainers();
    SetEnvironmentVariables();
  }

  private void SetEnvironmentVariables()
  {
    if (_dbContainer is null || _redisContainer is null)
    {
      throw new InvalidOperationException("Containers have not been initialized.");
    }

    var dbConnectionString = _dbContainer.GetConnectionString();

    // Set environment variables for the application to use the test containers
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

    // Connection strings for various modules
    Environment.SetEnvironmentVariable("ConnectionStrings__Database", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Administration", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Audit", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Communications", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Drafts", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Integrations", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Movies", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__RealTimeUpdates", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Reporting", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Users", dbConnectionString);
    Environment.SetEnvironmentVariable("ConnectionStrings__Cache", _redisContainer.GetConnectionString());
    Environment.SetEnvironmentVariable("ConnectionStrings__Queue", _rabbitMqContainer?.GetConnectionString());
    Environment.SetEnvironmentVariable("ConnectionStrings__Mongo", _mongoDbContainer?.GetConnectionString());

    // Keycloak settings
    Environment.SetEnvironmentVariable("KeyCloak__HealthUrl", "http://localhost:8080/health");

    // Serilog settings
    Environment.SetEnvironmentVariable("Serilog__MinimumLevel__Default", "Warning");
    Environment.SetEnvironmentVariable("Serilog__MinimumLevel__Override__Microsoft", "Warning");
  }

  private void InitializeAndStartContainers()
  {
    if (_containersInitialized)
    {
      return;
    }

    _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("screendrafts")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    _rabbitMqContainer = new RabbitMqBuilder()
      .WithImage("rabbitmq:3-management")
      .Build();

    _mongoDbContainer = new MongoDbBuilder()
      .WithImage("mongo:latest")
      .Build();

    Task.Run(async () =>
    {
      await _dbContainer.StartAsync();
      await _redisContainer.StartAsync();
      await _rabbitMqContainer.StartAsync();
      await _mongoDbContainer.StartAsync();

    }).GetAwaiter().GetResult();

    _containersInitialized = true;
  }


  /// <summary>
  /// Virtual method to allow module specific configuration.
  /// </summary>
  /// <param name="services">The services to configure.</param>
  protected virtual void ConfigureModuleServices(IServiceCollection services)
  {
    // Override in module specific factories if needed.
  }

  /// <summary>
  /// Virtual method to allow module-specific app configuration.
  /// </summary>
  /// <param name="app">The app builder.</param>
  protected virtual void ConfigureModuleApp(IApplicationBuilder app)
  {
    // Override in module specific factories if needed.
  }

  /// <summary>
  /// Virtual method to get module-specific DbContext types for migration application.
  /// </summary>
  /// <returns>Returns all known DbContext types.</returns>
  protected virtual IEnumerable<Type> GetDbContextTypes()
  {
    // Override in module specific factories to return only relevant DbContext types.
    var contextTypeNames = new[]
    {
      "ScreenDrafts.Modules.Administration.Infrastructure.Database.AdministrationDbContext",
      "ScreenDrafts.Modules.Audit.Infrastructure.Database.AuditDbContext",
      "ScreenDrafts.Modules.Communications.Infrastructure.Database.CommunicationsDbContext",
      "ScreenDrafts.Modules.Drafts.Infrastructure.Database.DraftsDbContext",
      "ScreenDrafts.Modules.Integrations.Infrastructure.Database.IntegrationsDbContext",
      "ScreenDrafts.Modules.Movies.Infrastructure.Database.MoviesDbContext",
      "ScreenDrafts.Modules.RealTimeUpdates.Infrastructure.Database.RealTimeUpdatesDbContext",
      "ScreenDrafts.Modules.Reporting.Infrastructure.Database.ReportingDbContext",
      "ScreenDrafts.Modules.Users.Infrastructure.Database.UsersDbContext",
    };

    return [.. AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => contextTypeNames.Contains(t.FullName, StringComparer.Ordinal))];
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.UseEnvironment("Testing");

    builder.ConfigureAppConfiguration((context, config) =>
    {
      if (!_containersInitialized)
      {
        throw new InvalidOperationException("Containers have not been initialized.");
      }

      config.Sources.Clear();

      config.AddEnvironmentVariables();

      config.AddInMemoryCollection(GetTestConfiguration());
    });

    builder.ConfigureTestServices(services =>
    {
      RemoveProblematicServices(services);
      ConfigureModuleServices(services);
    });
  }

  protected virtual Dictionary<string, string?> GetTestConfiguration()
  {
    return new Dictionary<string, string?>
    {
      ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "",
      ["Serilog:MinimumLevel:Default"] = "Warning",
      ["Serilog:MinimumLevel:Override:Microsoft"] = "Warning",
      ["Environment"] = "Testing"
    };
  }

  private static void RemoveProblematicServices(IServiceCollection services)
  {
    var hostServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
    foreach (var hostService in hostServices)
    {
      services.Remove(hostService);
    }

    services.RemoveAll<IHealthCheck>();
  }

  public async Task InitializeAsync()
  {
    if (_containersInitialized)
    {
      await ApplyMigrationsAsync();
    }
  }

  protected virtual async Task ApplyMigrationsAsync()
  {
    using var scope = Services.CreateScope();
    var dbContextTypes = GetDbContextTypes();

    foreach (var contextType in dbContextTypes)
    {
      var dbContext = scope.ServiceProvider.GetService(contextType) as DbContext;
      if (dbContext is not null)
      {
        await dbContext.Database.MigrateAsync();
      }
    }
  }

  public new async Task DisposeAsync()
  {
    await StopContainersAsync();
    await base.DisposeAsync();
  }

  protected virtual async Task StopContainersAsync()
  {
    if (!_disposed && _containersInitialized)
    {
      if (_dbContainer is not null)
      {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
      }

      if (_redisContainer is not null)
      {
        await _redisContainer.StopAsync();
        await _redisContainer.DisposeAsync();
      }

      if (_rabbitMqContainer is not null)
      {
        await _rabbitMqContainer.StopAsync();
        await _rabbitMqContainer.DisposeAsync();
      }

      if (_mongoDbContainer is not null)
      {
        await _mongoDbContainer.StopAsync();
        await _mongoDbContainer.DisposeAsync();
      }

      _disposed = true;
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && !_disposed)
    {
      if (_containersInitialized)
      {
        Task.Run(StopContainersAsync).GetAwaiter().GetResult();
      }
      else
      {
        _dbContainer?.ConfigureAwait(false).DisposeAsync();
        _redisContainer?.ConfigureAwait(false).DisposeAsync();
        _rabbitMqContainer?.ConfigureAwait(false).DisposeAsync();
        _mongoDbContainer?.ConfigureAwait(false).DisposeAsync();
      }

      _disposed = true;
    }
    base.Dispose(disposing);
  }
}
