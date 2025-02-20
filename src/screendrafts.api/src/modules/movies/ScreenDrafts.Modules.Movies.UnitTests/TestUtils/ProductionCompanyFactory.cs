using ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

namespace ScreenDrafts.Modules.Movies.UnitTests.TestUtils;

public static class ProductionCompanyFactory
{
  private static readonly Faker _faker = new();
  public static ProductionCompany CreateProductionCompany() =>
    ProductionCompany.Create(
      _faker.Company.CompanyName(),
      _faker.Random.AlphaNumeric(9));
}
