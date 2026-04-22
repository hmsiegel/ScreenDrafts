namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class AddRoleTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task AddRole_ShouldSucceed_WhenRoleIsNewAsync()
  {
    var result = await Sender.Send(new AddRoleCommand { Name = "Moderator" }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }

  [Fact]
  public async Task AddRole_ShouldFail_WhenRoleAlreadyExistsAsync()
  {
    const string roleName = "Editor";
    await Sender.Send(new AddRoleCommand { Name = roleName }, TestContext.Current.CancellationToken);

    var result = await Sender.Send(new AddRoleCommand { Name = roleName }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.RoleAlreadyExists(roleName));
  }
}
