namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class GetUserTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnError_WhenUserDoesNotExistAsync()
  {
    // Arrange
    var userId = Guid.NewGuid();

    // Act
    Result<UserResponse> userResult = await Sender.Send(new GetUserQuery(userId));

    // Assert
    userResult.Errors[0].Should().Be(UserErrors.NotFound(userId));
  }

  [Fact]
  public async Task Should_ReturnUser_WhenUserExistsAsync()
  {
    // Arrange
    Result<Guid> result = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));
    Guid userId = result.Value;

    // Act
    Result<UserResponse> userResult = await Sender.Send(new GetUserQuery(userId));

    // Assert
    userResult.IsSuccess.Should().BeTrue();
    userResult.Value.Should().NotBeNull();
  }
}
