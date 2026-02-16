namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class GetUserTests(UsersIntegrationTestWebAppFactory factory) : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnError_WhenUserDoesNotExistAsync()
  {
    // Arrange
    var userId = Guid.NewGuid();

    // Act
    var userResult = await Sender.Send(new GetByUserIdQuery(userId));

    // Assert
    userResult.Errors[0].Should().Be(UserErrors.NotFound(userId));
  }

  [Fact]
  public async Task Should_ReturnUser_WhenUserExistsAsync()
  {
    // Arrange
    Result<Guid> result = await Sender.Send(new RegisterUserCommand{
        Email = Faker.Internet.Email(),
        Password = Faker.Internet.Password(),
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    });
    Guid userId = result.Value;

    // Act
    var userResult = await Sender.Send(new GetByUserIdQuery(userId));

    // Assert
    userResult.IsSuccess.Should().BeTrue();
    userResult.Value.Should().NotBeNull();
  }
}
