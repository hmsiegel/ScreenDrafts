using ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

namespace ScreenDrafts.Modules.Movies.UnitTests.TestUtils;

public static class GenreFactory
{
  private static readonly Faker _faker = new();
  public static Genre CreateGenre() =>
    Genre.Create(
      _faker.Lorem.Word());
}
