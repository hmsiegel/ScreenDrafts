namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class ListPermissionsTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task ListPermissions_ShouldReturnEmpty_WhenNoPermissionsExistAsync()
  {
    var result = await Sender.Send(new ListPermissionsQuery(), TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.Permissions.Should().BeEmpty();
  }

  [Fact]
  public async Task ListPermissions_ShouldReturnAllPermissions_WhenPermissionsExistAsync()
  {
    await InsertPermissionAsync("drafts:read");
    await InsertPermissionAsync("drafts:write");

    var result = await Sender.Send(new ListPermissionsQuery(), TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.Permissions.Should().Contain(["drafts:read", "drafts:write"]);
  }
}
