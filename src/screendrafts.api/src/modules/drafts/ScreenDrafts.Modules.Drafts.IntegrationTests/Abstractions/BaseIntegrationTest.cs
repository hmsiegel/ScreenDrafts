namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public class BaseIntegrationTest : IDisposable
{
  private bool _disposedValue;
  protected static readonly Faker Faker = new();
  private readonly IServiceScope _serviceScope;
  protected readonly ISender Sender;
  protected readonly HttpClient HttpClient;
  protected readonly DraftsDbContext DbContext;

  public BaseIntegrationTest(IntegrationTestWebAppFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    _serviceScope = factory.Services.CreateScope();
    Sender = _serviceScope.ServiceProvider.GetRequiredService<ISender>();
    HttpClient = factory.CreateClient();
    DbContext = _serviceScope.ServiceProvider.GetRequiredService<DraftsDbContext>();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        _serviceScope.Dispose();
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
}
