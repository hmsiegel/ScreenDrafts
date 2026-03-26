namespace ScreenDrafts.Modules.Movies.UnitTests.TestUtils;

public static class MovieFactory
{
  private static readonly Faker _faker = new();

  public static Result<Media> CreateMovie() =>
    Media.Create(
      _faker.Random.AlphaNumeric(15),
      _faker.Company.CompanyName(),
      _faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture),
      _faker.Lorem.Paragraph(),
      _faker.Image.PicsumUrl(),
      _faker.Date.Past().ToString(CultureInfo.InvariantCulture),
      new Uri(_faker.Internet.Url()),
      _faker.Random.AlphaNumeric(9),
      _faker.Random.Int(1, 10000),
      _faker.Random.Int(1, 10000),
      _faker.Random.AlphaNumeric(15),
      MediaType.Movie);
}
