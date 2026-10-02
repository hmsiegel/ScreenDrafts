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
  }
}
