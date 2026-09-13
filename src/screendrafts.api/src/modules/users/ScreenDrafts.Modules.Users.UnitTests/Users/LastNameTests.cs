namespace ScreenDrafts.Modules.Users.UnitTests.Users;

public class LastNameTests : BaseTest
{
  [Fact]
  public void Create_WithValidName_ShouldReturnSuccess()
  {
    // Arrange
    var name = Faker.Name.LastName();

    // Act
    var result = LastName.Create(name);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Value.Should().Be(name);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Create_WithEmptyName_ShouldReturnFailure(string name)
  {
    // Act
    var result = LastName.Create(name);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(LastNameErrors.Empty);
  }

  [Fact]
  public void Create_WithNameLongerThanMaxLength_ShouldReturnFailure()
  {
    // Arrange
    var name = new string('a', LastName.MaxLength + 1);

    // Act
    var result = LastName.Create(name);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(LastNameErrors.TooLong);
  }

  [Fact]
  public void TwoLastNamesWithTheSameValue_ShouldBeEqual()
  {
    // Arrange
    var value = Faker.Name.LastName();

    // Act
    var a = LastName.Create(value).Value;
    var b = LastName.Create(value).Value;

    // Assert -- LastName is a record, so this holds by value rather than requiring
    // the same object reference; User.Update's no-op guard relies on that (see
    // UserTests for the regression this fixed).
    a.Should().Be(b);
  }
}
