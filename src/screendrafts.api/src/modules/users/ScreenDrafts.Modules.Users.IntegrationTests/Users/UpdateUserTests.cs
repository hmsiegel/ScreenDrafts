namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class UpdateUserTests(UsersIntegrationTestWebAppFactory factory) : UsersIntegrationTest(factory)
{
  public static readonly TheoryData<string, string, string> InvalidCommands =
      new()
      {
            { string.Empty, Faker.Name.FirstName(), Faker.Name.LastName() },
            { "invalid-public-id", "", Faker.Name.LastName() },
            { "invalid-public-id", Faker.Name.FirstName(), "" }
      };

  [Theory]
  [MemberData(memberName: nameof(InvalidCommands))]
  public async Task Should_ReturnError_WhenCommandIsNotValidAsync(
      string publicId,
      string firstName,
      string lastName)
  {
    // Arrange
    var command = new UpdateUserCommand(
        publicId,
        firstName,
        lastName);

    // Act
    Result result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
  }

  [Fact]
  public async Task Should_ReturnError_WhenUserDoesNotExistAsync()
  {
    // Arrange
    var publicId = "u_nonexistent";

    // Act
    Result updateResult = await Sender.Send(
        new UpdateUserCommand(
            publicId,
            Faker.Name.FirstName(),
            Faker.Name.LastName()));

    // Assert
    updateResult.IsFailure.Should().BeTrue();
    updateResult.Errors[0].Type.Should().Be(ErrorType.NotFound);
    updateResult.Errors[0].Should().Be(UserErrors.PublicIdNotFound(publicId));
  }

  [Fact]
  public async Task Should_ReturnSuccess_WhenUserExistsAsync()
  {
    // Arrange
    Result<Guid> registerResult = await Sender.Send(new RegisterUserCommand
    {
      Email = Faker.Internet.Email(),
      Password = Faker.Internet.Password(),
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    });

    var user = await Sender.Send(new GetByUserIdQuery(registerResult.Value));

    // Act
    Result updateResult = await Sender.Send(
        new UpdateUserCommand(
            user.Value.PublicId,
            Faker.Name.FirstName(),
            Faker.Name.LastName()));

    // Assert
    updateResult.IsSuccess.Should().BeTrue();
  }
}
