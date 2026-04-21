namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class AddPermissionToRoleTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task AddPermissionToRole_ShouldSucceed_WhenPermissionAndRoleExistAsync()
  {
    const string code = "reports:read";
    const string roleName = "Analyst";
    await InsertPermissionAsync(code);
    await InsertRoleAsync(roleName);

    var result = await Sender.Send(new AddPermissionToRoleCommand
    {
      PermissionCode = code,
      RoleName = roleName
    });

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }

  [Fact]
  public async Task AddPermissionToRole_ShouldFail_WhenPermissionDoesNotExistAsync()
  {
    const string roleName = "Viewer";
    await InsertRoleAsync(roleName);

    var result = await Sender.Send(new AddPermissionToRoleCommand
    {
      PermissionCode = "missing:permission",
      RoleName = roleName
    });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.PermissionNotFound("missing:permission"));
  }

  [Fact]
  public async Task AddPermissionToRole_ShouldFail_WhenAlreadyAssignedAsync()
  {
    const string code = "reports:write";
    const string roleName = "Manager";
    await InsertPermissionAsync(code);
    await InsertRoleAsync(roleName);
    await Sender.Send(new AddPermissionToRoleCommand { PermissionCode = code, RoleName = roleName });

    var result = await Sender.Send(new AddPermissionToRoleCommand
    {
      PermissionCode = code,
      RoleName = roleName
    });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.PermissionAlreadyAssigned(roleName, code));
  }
}
