namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class RemoveRoleFromUserTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task RemoveRoleFromUser_ShouldFail_WhenUserDoesNotExistAsync()
  {
    var result = await Sender.Send(new RemoveRoleFromUserCommand
    {
      UserPublicId = "u_nonexistent00000000000",
      RoleName = "Guest"
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.UserNotFound("u_nonexistent00000000000"));
  }

  [Fact]
  public async Task RemoveRoleFromUser_ShouldSucceed_WhenUserHasRoleAsync()
  {
    var (userId, publicId) = await InsertUserAsync();
    const string roleName = "Guest";
    await InsertRoleAsync(roleName);

    var connectionFactory = GetService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync(TestContext.Current.CancellationToken);
    await connection.ExecuteAsync(
      "INSERT INTO administration.user_roles (user_id, role_name) VALUES (@UserId, @RoleName)",
      new { UserId = userId, RoleName = roleName });

    var result = await Sender.Send(new RemoveRoleFromUserCommand
    {
      UserPublicId = publicId,
      RoleName = roleName
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveRoleFromUser_ShouldSucceed_WhenUserDoesNotHaveRoleAsync()
  {
    var (_, publicId) = await InsertUserAsync();
    const string roleName = "Patron";
    await InsertRoleAsync(roleName);

    var result = await Sender.Send(new RemoveRoleFromUserCommand
    {
      UserPublicId = publicId,
      RoleName = roleName
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }
}
