namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;
public class MoviesIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  protected override IEnumerable<Type> GetDbContextTypes()
  {
    return [typeof(MoviesDbContext)];
  }
}
