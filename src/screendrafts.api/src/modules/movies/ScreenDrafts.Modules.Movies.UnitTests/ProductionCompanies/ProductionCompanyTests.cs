namespace ScreenDrafts.Modules.Movies.UnitTests.ProductionCompanies;

public class ProductionCompanyTests : BaseTest
{
  [Fact]
  public void CreateProductionCompany_WithValidData_ShouldReturnSuccessResult()
  {
    // Arrange
    var productionCompanyName = Faker.Company.CompanyName();
    var imdbId = Faker.Random.String2(1, 50);
    var tmdbId = Faker.Random.Int(1, 1000);
    // Act
    var productionCompany = ProductionCompany.Create(productionCompanyName, imdbId, tmdbId);
    // Assert
    productionCompany.Should().NotBeNull();
    productionCompany.Name.Should().Be(productionCompanyName);
  }

  [Fact]
  public void CreateProductionCompany_WithInvalidData_ShouldThrowException()
  {
    // Arrange
    var productionCompanyName = string.Empty;
    var imdbId = Faker.Random.String2(1, 50);
    var tmdbId = Faker.Random.Int(1, 1000);
    // Act
    Action act = () => ProductionCompany.Create(productionCompanyName, imdbId, tmdbId);
    // Assert
    act.Should().Throw<ArgumentException>();
  }
}
