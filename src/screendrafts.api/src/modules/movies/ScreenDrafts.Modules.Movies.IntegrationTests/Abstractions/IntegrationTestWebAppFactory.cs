namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
      .WithImage("postgres:latest")
      .WithDatabase("screendrafts")
      .WithUsername("postgres")
      .WithPassword("postgres")
      .Build();

  private readonly RedisContainer _redisContainer = new RedisBuilder()
      .WithImage("redis:latest")
      .Build();
  
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    Environment.SetEnvironmentVariable("ConnectionStrings:Database", _dbContainer.GetConnectionString());
    Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());

    base.ConfigureWebHost(builder);
  }
  public async Task InitializeAsync()
  {
    await _dbContainer.StartAsync();
    await _redisContainer.StartAsync();
  }

  public new async Task DisposeAsync()
  {
    await _dbContainer.StopAsync();
    await _redisContainer.StopAsync();

    await _dbContainer.DisposeAsync();
    await _redisContainer.DisposeAsync();
  }
}
