namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

public class DraftsIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  protected override IEnumerable<Type> GetDbContextTypes()
  {
    return [typeof(DraftsDbContext)];
  }
}
