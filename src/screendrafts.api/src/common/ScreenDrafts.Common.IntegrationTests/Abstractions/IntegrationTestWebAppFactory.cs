namespace ScreenDrafts.Common.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private PostgreSqlContainer? _dbContainer;
  private RedisContainer? _redisContainer;
  private RabbitMqContainer? _rabbitMqContainer;
  private MongoDbContainer? _mongoDbContainer;

  private bool _disposed;
  private bool _containersInitialized;

  private void CreateContainers()
  {
    _dbContainer = new PostgreSqlBuilder("postgres:latest")
      .WithDatabase("screendrafts")
      .WithUsername("postgres")
      .WithPassword("postgres")
      .Build();

    _redisContainer = new RedisBuilder("redis:latest").Build();

    _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:4-management").Build();

    _mongoDbContainer = new MongoDbBuilder("mongo:latest").Build();
  }

  private void SetEnvironmentVariables()
  {
    if (_dbContainer is null || _redisContainer is null)
    {
      throw new InvalidOperationException("Containers have not been initialized.");
    }

    var dbConnectionString = _dbContainer.GetConnectionString();

    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

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
    Environment.SetEnvironmentVariable(
      "ConnectionStrings__Cache",
      _redisContainer.GetConnectionString()
    );
    Environment.SetEnvironmentVariable(
      "ConnectionStrings__Queue",
      _rabbitMqContainer?.GetConnectionString()
    );
    Environment.SetEnvironmentVariable(
      "ConnectionStrings__Mongo",
      _mongoDbContainer?.GetConnectionString()
    );

    Environment.SetEnvironmentVariable("KeyCloak__HealthUrl", "http://localhost:8080/health");
    Environment.SetEnvironmentVariable("Serilog__MinimumLevel__Default", "Warning");
    Environment.SetEnvironmentVariable("Serilog__MinimumLevel__Override__Microsoft", "Warning");
  }

  /// <summary>
  /// Virtual method to allow module specific configuration.
  /// </summary>
  protected virtual void ConfigureModuleServices(IServiceCollection services) { }

  /// <summary>
  /// Virtual method to allow module-specific app configuration.
  /// </summary>
  protected virtual void ConfigureModuleApp(IApplicationBuilder app) { }

  /// <summary>
  /// Virtual method to get module-specific DbContext types for migration application.
  /// </summary>
  protected virtual IEnumerable<Type> GetDbContextTypes()
  {
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

    return
    [
      .. AppDomain
        .CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => contextTypeNames.Contains(t.FullName, StringComparer.Ordinal)),
    ];
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.UseEnvironment("Testing");

    builder.ConfigureAppConfiguration(
      (context, config) =>
      {
        if (!_containersInitialized)
        {
          throw new InvalidOperationException("Containers have not been initialized.");
        }

        config.Sources.Clear();
        config.AddEnvironmentVariables();
        config.AddInMemoryCollection(GetTestConfiguration());
      }
    );

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
      ["Environment"] = "Testing",
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

    services.RemoveAll<IEventBus>();
    services.AddSingleton<IEventBus, NoOpEventBus>();
  }

  private sealed class NoOpEventBus : IEventBus
  {
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
      where T : IIntegrationEvent => Task.CompletedTask;
  }

  public async Task InitializeAsync()
  {
    CreateContainers();

    await Task.WhenAll(
      _dbContainer!.StartAsync(),
      _redisContainer!.StartAsync(),
      _rabbitMqContainer!.StartAsync(),
      _mongoDbContainer!.StartAsync()
    );

    _containersInitialized = true;

    SetEnvironmentVariables();

    await ApplyMigrationsAsync();
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
        _dbContainer?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _redisContainer?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _rabbitMqContainer?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _mongoDbContainer?.DisposeAsync().AsTask().GetAwaiter().GetResult();
      }

      _disposed = true;
    }
    base.Dispose(disposing);
  }

  ValueTask IAsyncLifetime.InitializeAsync()
  {
    return new ValueTask(InitializeAsync());
  }
}
