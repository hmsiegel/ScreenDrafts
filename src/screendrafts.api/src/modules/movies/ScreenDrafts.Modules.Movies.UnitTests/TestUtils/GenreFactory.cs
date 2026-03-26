namespace ScreenDrafts.Modules.Movies.UnitTests.TestUtils;

public static class GenreFactory
{
  private static readonly Faker _faker = new();
  public static Genre CreateGenre() =>
    Genre.Create(
      _faker.Lorem.Word(),
      _faker.Random.Int(1, int.MaxValue));
}
