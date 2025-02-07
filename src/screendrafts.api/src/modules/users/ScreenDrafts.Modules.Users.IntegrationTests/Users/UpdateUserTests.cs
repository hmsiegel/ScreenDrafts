namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class UpdateUserTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
  public static readonly TheoryData<Guid, string, string> InvalidCommands =
    new()
    {
      { Guid.Empty, Faker.Name.FirstName(), Faker.Name.LastName() },
      { Guid.NewGuid(), "", Faker.Name.LastName() },
      { Guid.NewGuid(), Faker.Name.FirstName(), "" }
    };

  [Theory]
  [MemberData(memberName: nameof(InvalidCommands))]
  public async Task Should_ReturnError_WhenCommandIsNotValidAsync(
    Guid userId,
    string firstName,
    string lastName)
  {
    // Arrange
    var command = new UpdateUserCommand(
      userId,
      firstName,
      lastName);

    // Act
    Result result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.NotFound);
  }

  [Fact]
  public async Task Should_ReturnError_WhenUserDoesNotExistAsync()
  {
    // Arrange
    var userId = Guid.NewGuid();

    // Act
    Result updateResult = await Sender.Send(
        new UpdateUserCommand(
          userId,
          Faker.Name.FirstName(),
          Faker.Name.LastName()));

    // Assert
    updateResult.Errors[0].Should().Be(UserErrors.NotFound(userId));
  }

  [Fact]
  public async Task Should_ReturnSuccess_WhenUserExistsAsync()
  {
    // Arrange
    Result<Guid> result = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));

    Guid userId = result.Value;

    // Act
    Result updateResult = await Sender.Send(
        new UpdateUserCommand(userId, Faker.Name.FirstName(), Faker.Name.LastName()));

    // Assert
    updateResult.IsSuccess.Should().BeTrue();
  }
}
