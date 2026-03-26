namespace ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

public static class MovieFactory
{
    private static readonly Faker _faker = new();

  public static Result<Media> CreateMovie() => Media.Create(
    _faker.Random.AlphaNumeric(15),
      _faker.Company.CompanyName(),
      _faker.Random.Int(1900, 2022).ToString(CultureInfo.InvariantCulture),
      _faker.Lorem.Paragraph(),
      _faker.Image.LoremFlickrUrl(),
      _faker.Date.Between(new DateTime(
        1900,
        1,
        1,
        0,
        0,
        0,
        DateTimeKind.Utc), DateTime.UtcNow).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
      null,
      _faker.Random.Replace("tt#######"),
      _faker.Random.Int(1, 1000),
      _faker.Random.Int(1, 1000),
      _faker.Random.AlphaNumeric(15),
      MediaType.Movie);

    public static Result<Genre> CreateGenre() => Genre.Create(_faker.Music.Genre(), _faker.Random.Int(1, 1000));

    public static Result<ScreenDrafts.Modules.Movies.Domain.Medias.Person> CreatePerson() => ScreenDrafts.Modules.Movies.Domain.Medias.Person.Create(
      _faker.Random.String2(9),
      _faker.Name.FullName(),
      _faker.Random.Int(1, 1000));

    public static Result<ProductionCompany> CreateProductionCompany() => ProductionCompany.Create(
      _faker.Company.CompanyName(),
      _faker.Random.String2(9),
      _faker.Random.Int(1, 1000));
}
