namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;
public static class MovieFactory
{
  private static readonly Faker _faker = new();

  public static Result<Movie> CreateMovie() =>
    Movie.Create(_faker.Commerce.ProductName(), _faker.Random.AlphaNumeric(9), Guid.NewGuid());

}
