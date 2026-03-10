using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ScreenDrafts.Modules.Integrations.PublicApi;
using ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;

public class MoviesIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  public FakeIntegrationsApi FakeIntegrationsApi { get; } = new();

  protected override IEnumerable<Type> GetDbContextTypes()
  {
    return [typeof(MoviesDbContext)];
  }

  protected override void ConfigureModuleServices(IServiceCollection services)
  {
    services.RemoveAll<IIntegrationsApi>();
    services.AddSingleton<IIntegrationsApi>(FakeIntegrationsApi);
  }
}
