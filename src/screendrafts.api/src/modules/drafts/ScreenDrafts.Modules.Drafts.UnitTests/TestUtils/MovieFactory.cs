using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;
public static class MovieFactory
{
  private static readonly Faker _faker = new();

  public static Result<Movie> CreateMovie() =>
    Movie.Create(
      _faker.Commerce.ProductName(),
      _faker.Lorem.Word(),
      MediaType.Movie,
      _faker.Internet.Random.Guid());
}
