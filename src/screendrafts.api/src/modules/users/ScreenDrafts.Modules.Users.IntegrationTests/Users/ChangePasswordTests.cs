namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class ChangePasswordTests(UsersIntegrationTestWebAppFactory factory) : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenCurrentPasswordIsCorrectAsync()
  {
    var email = Faker.Internet.Email();
    const string currentPassword = "Test@123456";

    var registerResult = await Sender.Send(new RegisterUserCommand
    {
      Email = email,
      Password = currentPassword,
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    }, TestContext.Current.CancellationToken);

    var userResult = await Sender.Send(
      new GetByUserIdQuery(registerResult.Value),
      TestContext.Current.CancellationToken);

    var result = await Sender.Send(new ChangePasswordCommand
    {
      PublicId = userResult.Value.PublicId,
      CurrentPassword = currentPassword,
      Password = "NewTest@789012"
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Should_ReturnError_WhenCurrentPasswordIsIncorrectAsync()
  {
    var registerResult = await Sender.Send(new RegisterUserCommand
    {
      Email = Faker.Internet.Email(),
      Password = "Test@123456",
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    }, TestContext.Current.CancellationToken);

    var userResult = await Sender.Send(
      new GetByUserIdQuery(registerResult.Value),
      TestContext.Current.CancellationToken);

    var result = await Sender.Send(new ChangePasswordCommand
    {
      PublicId = userResult.Value.PublicId,
      CurrentPassword = "WrongPassword@999",
      Password = "NewTest@789012"
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(IdentityProviderErrors.InvalidCurrentPassword);
  }

  [Fact]
  public async Task Should_ReturnNotFound_WhenUserDoesNotExistAsync()
  {
    var publicId = "u_doesnotexist12";

    var result = await Sender.Send(new ChangePasswordCommand
    {
      PublicId = publicId,
      CurrentPassword = "Test@123456",
      Password = "NewTest@789012"
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    result.Errors[0].Should().Be(UserErrors.PublicIdNotFound(publicId));
  }
}
