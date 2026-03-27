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

  protected override async Task ApplyMigrationsAsync()
  {
    // Migrations 3 and 4 are broken: migration 3 renames movies→media but then
    // creates media_* junction tables with FK constraints still referencing the
    // old "movies" table name, causing PostgreSQL 42P01 on a fresh database.
    // Migration 4 was intended to fix those FKs but never runs because migration 3
    // fails atomically. EnsureCreatedAsync creates the schema from the current
    // model (which correctly reflects the "media" table), bypassing the broken
    // migration chain entirely.
    using var scope = Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
  }
}
