namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class GetUserRolesTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task GetUserRoles_ShouldFail_WhenUserDoesNotExistAsync()
  {
    var result = await Sender.Send(new GetUserRolesQuery { PublicId = "u_nonexistent00000000000" });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.UserNotFound("u_nonexistent00000000000"));
  }

  [Fact]
  public async Task GetUserRoles_ShouldFail_WhenUserHasNoRolesAsync()
  {
    var (_, publicId) = await InsertUserAsync();

    var result = await Sender.Send(new GetUserRolesQuery { PublicId = publicId });

    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task GetUserRoles_ShouldReturnRoles_WhenUserHasRolesAsync()
  {
    var (userId, publicId) = await InsertUserAsync();
    const string roleName = "Guest";
    await InsertRoleAsync(roleName);

    var connectionFactory = GetService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync();
    await connection.ExecuteAsync(
      "INSERT INTO administration.user_roles (user_id, role_name) VALUES (@UserId, @RoleName)",
      new { UserId = userId, RoleName = roleName });

    var result = await Sender.Send(new GetUserRolesQuery { PublicId = publicId });

    result.IsSuccess.Should().BeTrue();
    result.Value.Roles.Should().Contain(roleName);
  }
}
