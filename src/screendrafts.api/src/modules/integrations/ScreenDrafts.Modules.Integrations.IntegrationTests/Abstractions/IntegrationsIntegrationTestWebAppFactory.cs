namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Abstractions;

public sealed class IntegrationsIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  public FakeTmdbService FakeTmdbService { get; } = new();
  public FakeOmdbService FakeOmdbService { get; } = new();

  protected override IEnumerable<Type> GetDbContextTypes()
  {
    return [typeof(IntegrationsDbContext)];
  }

  protected override void ConfigureModuleServices(IServiceCollection services)
  {
    services.RemoveAll<ITmdbService>();
    services.AddSingleton<ITmdbService>(FakeTmdbService);
    services.RemoveAll<IOmdbService>();
    services.AddSingleton<IOmdbService>(FakeOmdbService);
  }

  protected override Dictionary<string, string?> GetTestConfiguration()
  {
    var config = base.GetTestConfiguration();
    config["Integrations:Tmdb:BaseAddress"] = "https://api.themoviedb.org/3/";
    config["Integrations:Tmdb:BaseImageAddress"] = "https://image.tmdb.org/t/p/";
    config["Integrations:Tmdb:AccessToken"] = "fake-token-for-testing";
    config["Integrations:Tmdb:TrailerPlaceholder"] = "https://www.imdb.com/videoplayer/placeholder";
    config["Integrations:YouTube:BaseAddress"] = "https://www.googleapis.com/youtube/v3/";
    return config;
  }
}
