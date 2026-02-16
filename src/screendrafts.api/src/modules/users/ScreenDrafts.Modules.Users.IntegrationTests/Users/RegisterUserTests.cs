namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class RegisterUserTests(UsersIntegrationTestWebAppFactory factory)
    : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Register_ShouldRegisterNewUser_WhenValidRequestAsync()
  {
    // Arrange
    var command = new Features.Users.Register.RegisterUserCommand
    {
      Email = Faker.Internet.Email(),
      Password = "Test@123456",
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBe(Guid.Empty);
  }

  [Fact]
  public async Task Register_ShouldFail_WhenEmailAlreadyExistsAsync()
  {
    // Arrange
    var email = Faker.Internet.Email();
    var command1 = new Features.Users.Register.RegisterUserCommand
    {
        Email = email,
        Password = "Test@123456",
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    };
    await Sender.Send(command1);

    var command2 = new Features.Users.Register.RegisterUserCommand
    {
        Email = email,
        Password = "Test@123456",
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    };

    // Act
    var result = await Sender.Send(command2);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Register_ShouldFail_WhenPasswordTooShortAsync()
  {
    // Arrange
    var command = new Features.Users.Register.RegisterUserCommand
    {
        Email = Faker.Internet.Email(),
        Password = "short",
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
