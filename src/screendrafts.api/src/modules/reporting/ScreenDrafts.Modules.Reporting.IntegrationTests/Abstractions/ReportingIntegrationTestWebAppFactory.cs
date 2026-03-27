namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

public class ReportingIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  protected override IEnumerable<Type> GetDbContextTypes()
  {
    return [typeof(ReportingDbContext)];
  }
}
