namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class AddPermissionTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task AddPermission_ShouldSucceed_WhenPermissionIsNewAsync()
  {
    var command = new AddPermissionCommand { Code = "movies:read" };

    var result = await Sender.Send(command);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddPermission_ShouldFail_WhenPermissionAlreadyExistsAsync()
  {
    const string code = "movies:write";
    await Sender.Send(new AddPermissionCommand { Code = code });

    var result = await Sender.Send(new AddPermissionCommand { Code = code });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.PermissionAlreadyExists(code));
  }
}
