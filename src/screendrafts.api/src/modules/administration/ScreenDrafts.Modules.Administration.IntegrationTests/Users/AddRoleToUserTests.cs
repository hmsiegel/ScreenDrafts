namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public class AddRoleToUserTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task AddRoleToUser_ShouldFail_WhenUserDoesNotExistAsync()
  {
    const string roleName = "Guest";
    await InsertRoleAsync(roleName);

    var result = await Sender.Send(new AddRoleToUserCommand
    {
      UserPublicId = "u_nonexistent00000000000",
      RoleName = roleName
    });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(AdministrationErrors.UserNotFound("u_nonexistent00000000000"));
  }

  [Fact]
  public async Task AddRoleToUser_ShouldSucceed_WhenUserAndRoleExistAsync()
  {
    var (_, publicId) = await InsertUserAsync();
    const string roleName = "Member";
    await InsertRoleAsync(roleName);

    var result = await Sender.Send(new AddRoleToUserCommand
    {
      UserPublicId = publicId,
      RoleName = roleName
    });

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }

  [Fact]
  public async Task AddRoleToUser_ShouldBeIdempotent_WhenCalledTwiceAsync()
  {
    var (_, publicId) = await InsertUserAsync();
    const string roleName = "Patron";
    await InsertRoleAsync(roleName);

    await Sender.Send(new AddRoleToUserCommand { UserPublicId = publicId, RoleName = roleName });
    var result = await Sender.Send(new AddRoleToUserCommand { UserPublicId = publicId, RoleName = roleName });

    result.IsSuccess.Should().BeTrue();
  }
}
