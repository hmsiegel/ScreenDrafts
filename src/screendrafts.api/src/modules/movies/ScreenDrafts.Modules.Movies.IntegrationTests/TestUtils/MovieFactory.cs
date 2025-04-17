namespace ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

public static class MovieFactory
{
    private static readonly Faker _faker = new();

    public static Result<Movie> CreateMovie() => Movie.Create(
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
      new Uri(_faker.Internet.Url()),
      _faker.Random.Replace("tt#######"));

    public static Result<Genre> CreateGenre() => Genre.Create(_faker.Music.Genre());

    public static Result<Domain.Movies.Person> CreatePerson() => Domain.Movies.Person.Create(
      _faker.Random.String2(9),
      _faker.Name.FullName());

    public static Result<ProductionCompany> CreateProductionCompany() => ProductionCompany.Create(
      _faker.Company.CompanyName(),
      _faker.Random.String2(9));
}
