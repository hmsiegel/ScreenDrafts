using Microsoft.Extensions.DependencyInjection.Extensions;
using ScreenDrafts.Modules.Movies.PublicApi;

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

public class ReportingIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  protected override IEnumerable<Type> GetDbContextTypes()
  {
    return [typeof(ReportingDbContext)];
  }

  protected override void ConfigureModuleServices(IServiceCollection services)
  {
    base.ConfigureModuleServices(services);

    // HTTP-level tests authenticate with permissions supplied directly on the request, and the
    // audit writer is stubbed, so neither Keycloak nor the Users/Audit schemas are required.
    services.AddTestAuthentication().AddNoOpAuditWriter();

    // The Movies schema is not migrated in Reporting tests, so title lookups are stubbed.
    services.RemoveAll<IMovieTitleReader>();
    services.AddSingleton<IMovieTitleReader, StubMovieTitleReader>();
  }

  private sealed class StubMovieTitleReader : IMovieTitleReader
  {
    public Task<IReadOnlyDictionary<string, string>> GetTitlesByPublicIdsAsync(
      IEnumerable<string> publicIds,
      CancellationToken cancellationToken = default
    ) => Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());

    public Task<IReadOnlyList<string>> GetPublicIdsByTmdbIdsAsync(
      IReadOnlyList<int> tmdbIds,
      CancellationToken cancellationToken = default
    ) => Task.FromResult<IReadOnlyList<string>>([]);
  }
}
