using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Movies.UnitTests.TestUtils;

public static class MovieFactory
{
  private static readonly Faker _faker = new();

  public static Result<Movie> CreateMovie() =>
    Movie.Create(
      _faker.Company.CompanyName(),
      _faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture),
      _faker.Lorem.Paragraph(),
      _faker.Image.PicsumUrl(),
      _faker.Date.Past().ToString(CultureInfo.InvariantCulture),
      new Uri(_faker.Internet.Url()),
      _faker.Random.AlphaNumeric(9));
}
