namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class GetPermissionByCodeTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task GetPermissionByCode_ShouldFail_WhenPermissionDoesNotExistAsync()
  {
    var result = await Sender.Send(new GetPermissionByCodeQuery { Code = "nonexistent:permission" }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.PermissionNotFound("nonexistent:permission"));
  }

  [Fact]
  public async Task GetPermissionByCode_ShouldReturnPermission_WhenPermissionExistsAsync()
  {
    const string code = "users:read";
    await InsertPermissionAsync(code);

    var result = await Sender.Send(new GetPermissionByCodeQuery { Code = code }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.Code.Should().Be(code);
    result.Value.Roles.Should().BeEmpty();
  }

  [Fact]
  public async Task GetPermissionByCode_ShouldReturnAssignedRoles_WhenRolesAreAssignedAsync()
  {
    const string code = "users:write";
    const string roleName = "Admin";
    await InsertPermissionAsync(code);
    await InsertRoleAsync(roleName);

    await Sender.Send(new AddPermissionToRoleCommand
    {
      PermissionCode = code,
      RoleName = roleName
    }, TestContext.Current.CancellationToken);

    var result = await Sender.Send(new GetPermissionByCodeQuery { Code = code }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.Roles.Should().Contain(roleName);
  }
}
