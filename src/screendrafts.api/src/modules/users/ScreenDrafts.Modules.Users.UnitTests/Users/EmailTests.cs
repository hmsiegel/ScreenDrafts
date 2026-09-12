namespace ScreenDrafts.Modules.Users.UnitTests.Users;

public class EmailTests : BaseTest
{
  [Fact]
  public void Create_WithValidEmail_ShouldReturnSuccess()
  {
    // Arrange
    var email = Faker.Internet.Email();

    // Act
    var result = Email.Create(email);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Value.Should().Be(email);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Create_WithEmptyEmail_ShouldReturnFailure(string email)
  {
    // Act
    var result = Email.Create(email);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailErrors.Empty);
  }

  [Fact]
  public void Create_WithNullEmail_ShouldReturnFailureRatherThanThrow()
  {
    // Arrange, Act -- regression: this used to reach `e.Length` inside a
    // non-short-circuiting Result.Ensure(value, tuples[]) call with e == null and
    // throw a NullReferenceException instead of failing gracefully. Email.Create now
    // uses the same short-circuiting chained .Ensure() style as FirstName/LastName,
    // so a null value is caught by Result.Create before any predicate runs.
    var act = () => Email.Create(null);

    // Assert
    act.Should().NotThrow();
    act().IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_WithEmailLongerThanMaxLength_ShouldReturnFailure()
  {
    // Arrange -- must still contain exactly one '@' so only the length rule fires.
    var email = $"{new string('a', Email.MaxLength)}@example.com";

    // Act
    var result = Email.Create(email);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailErrors.TooLong);
  }

  [Theory]
  [InlineData("not-an-email")]
  [InlineData("two@@signs.com")]
  [InlineData("a@b@c.com")]
  public void Create_WithoutExactlyOneAtSign_ShouldReturnFailure(string email)
  {
    // Act
    var result = Email.Create(email);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailErrors.Invalid);
  }

  [Fact]
  public void TwoEmailsWithTheSameValue_ShouldBeEqual()
  {
    // Arrange
    var value = Faker.Internet.Email();

    // Act
    var a = Email.Create(value).Value;
    var b = Email.Create(value).Value;

    // Assert -- Email is a record, so User.Update-style no-op comparisons rely on
    // this structural equality rather than reference equality.
    a.Should().Be(b);
  }
}
