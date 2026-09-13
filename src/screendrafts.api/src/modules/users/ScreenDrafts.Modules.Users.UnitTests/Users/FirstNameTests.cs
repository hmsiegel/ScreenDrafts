namespace ScreenDrafts.Modules.Users.UnitTests.Users;

public class FirstNameTests : BaseTest
{
  [Fact]
  public void Create_WithValidName_ShouldReturnSuccess()
  {
    // Arrange
    var name = Faker.Name.FirstName();

    // Act
    var result = FirstName.Create(name);

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
    var result = FirstName.Create(name);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(FirstNameErrors.Empty);
  }

  [Fact]
  public void Create_WithNameLongerThanMaxLength_ShouldReturnFailure()
  {
    // Arrange
    var name = new string('a', FirstName.MaxLength + 1);

    // Act
    var result = FirstName.Create(name);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(FirstNameErrors.TooLong);
  }

  [Fact]
  public void TwoFirstNamesWithTheSameValue_ShouldBeEqual()
  {
    // Arrange
    var value = Faker.Name.FirstName();

    // Act
    var a = FirstName.Create(value).Value;
    var b = FirstName.Create(value).Value;

    // Assert -- FirstName is a record, so this holds by value rather than requiring
    // the same object reference; User.Update's no-op guard relies on that (see
    // UserTests for the regression this fixed).
    a.Should().Be(b);
  }
}
