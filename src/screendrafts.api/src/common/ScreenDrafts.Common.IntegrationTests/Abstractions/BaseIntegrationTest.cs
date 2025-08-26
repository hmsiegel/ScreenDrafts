namespace ScreenDrafts.Common.IntegrationTests.Abstractions;

public abstract partial class BaseIntegrationTest<TDbContext> : IDisposable, IAsyncLifetime
  where TDbContext : DbContext
{
  private bool _disposedValue;
  protected static Faker Faker => TestFakerProvider.Faker;

  protected IServiceScope ServiceScope { get; }
  protected ISender Sender { get; }
  protected HttpClient HttpClient { get; }
  protected TDbContext DbContext { get; }
  protected IntegrationTestWebAppFactory Factory { get; }

  protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    Factory = factory;
    ServiceScope = factory.Services.CreateScope();
    Sender = ServiceScope.ServiceProvider.GetRequiredService<ISender>();
    HttpClient = factory.CreateClient();
    DbContext = ServiceScope.ServiceProvider.GetRequiredService<TDbContext>();
  }

  protected abstract Task ClearDatabaseAsync();

  protected T GetService<T>()
      where T : notnull
  {
    return ServiceScope.ServiceProvider.GetRequiredService<T>();
  }

  protected T? GetOptionalService<T>()
      where T : class
  {
    return ServiceScope.ServiceProvider.GetService<T>();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        ServiceScope.Dispose();
        HttpClient.Dispose();
        DbContext.Dispose();
      }

      _disposedValue = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  public async Task DisposeAsync()
  {
    await ClearDatabaseAsync();
    await OnDisposeAsync();
    Dispose();
  }

  protected virtual Task OnDisposeAsync()
  {
    return Task.CompletedTask;
  }

  public async Task InitializeAsync()
  {
    await ClearDatabaseAsync();
    await OnInitializeAsync();
  }

  protected virtual Task OnInitializeAsync()
  {
    return Task.CompletedTask;
  }
}
