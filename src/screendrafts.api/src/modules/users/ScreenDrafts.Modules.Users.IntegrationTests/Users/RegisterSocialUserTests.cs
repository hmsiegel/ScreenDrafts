namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

/// <summary>
/// RegisterSocialUserCommandHandler had no coverage at all before this file. Unlike
/// RegisterUserCommand, it has real defaulting logic (FirstName falls back to
/// "User", LastName falls back to the local part of the email) and is meant to be
/// idempotent by IdentityId (an OAuth callback can legitimately fire more than once
/// for the same social login) -- both are exercised here.
/// </summary>
public sealed class RegisterSocialUserTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task RegisterSocialUser_WithValidData_ShouldCreateUserAsync()
  {
    // Arrange
    var command = new RegisterSocialUserCommand
    {
      Email = Faker.Internet.Email(),
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName(),
      IdentityId = Guid.NewGuid().ToString(),
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task RegisterSocialUser_CalledTwiceWithTheSameIdentityId_ShouldReturnTheSameUserAsync()
  {
    // Arrange -- an OAuth callback can legitimately fire more than once; this must
    // stay a find-not-create on the second call, not attempt (and fail) a second
    // insert.
    var command = new RegisterSocialUserCommand
    {
      Email = Faker.Internet.Email(),
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName(),
      IdentityId = Guid.NewGuid().ToString(),
    };
    var first = await Sender.Send(command, TestContext.Current.CancellationToken);
    first.IsSuccess.Should().BeTrue();

    // Act
    var second = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    second.IsSuccess.Should().BeTrue();
    second.Value.Should().Be(first.Value);
  }

  [Fact]
  public async Task RegisterSocialUser_WithMissingFirstName_ShouldDefaultToUserAsync()
  {
    // Arrange
    var command = new RegisterSocialUserCommand
    {
      Email = Faker.Internet.Email(),
      FirstName = string.Empty,
      LastName = Faker.Name.LastName(),
      IdentityId = Guid.NewGuid().ToString(),
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var user = await Sender.Send(
      new GetByPublicIdQuery(result.Value),
      TestContext.Current.CancellationToken
    );
    user.IsSuccess.Should().BeTrue();
    user.Value.FirstName.Should().Be("User");
  }

  [Fact]
  public async Task RegisterSocialUser_WithMissingLastName_ShouldDefaultToTheEmailLocalPartAsync()
  {
    // Arrange
    var email = $"{Faker.Random.AlphaNumeric(10)}@example.com";
    var command = new RegisterSocialUserCommand
    {
      Email = email,
      FirstName = Faker.Name.FirstName(),
      LastName = string.Empty,
      IdentityId = Guid.NewGuid().ToString(),
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var user = await Sender.Send(
      new GetByPublicIdQuery(result.Value),
      TestContext.Current.CancellationToken
    );
    user.IsSuccess.Should().BeTrue();
    user.Value.LastName.Should().Be(email.Split('@')[0]);
  }

  [Fact]
  public async Task RegisterSocialUser_WithInvalidEmail_ShouldFailAsync()
  {
    // Arrange
    var command = new RegisterSocialUserCommand
    {
      Email = "not-an-email",
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName(),
      IdentityId = Guid.NewGuid().ToString(),
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == EmailErrors.Invalid.Code);
  }
}
