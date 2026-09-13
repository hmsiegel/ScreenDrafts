namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public sealed class SendPasswordResetTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task SendPasswordReset_WhenUserDoesNotExist_ShouldFailAsync()
  {
    // Arrange
    const string publicId = "u_nonexistent00000000000";

    // Act
    var result = await Sender.Send(
      new SendPasswordResetCommand { PublicId = publicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.UserNotFound(publicId));
  }
}
