namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class GetUserPermissionTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnError_WhenUserDoesNotExistAsync()
  {
    // Arrange
    string identityId = Guid.NewGuid().ToString();

    // Act
    Result<PermissionsResponse> permissionsResult = await Sender.Send(new Features.Users.GetUserPermissions.GetUserPermissionsQuery(identityId), TestContext.Current.CancellationToken);

    // Assert
    permissionsResult.Errors[0].Should().Be(UserErrors.NotFound(identityId));
  }

  [Fact]
  public async Task Should_ReturnPermissions_WhenUserExistsAsync()
  {
    // Arrange
    var userId = await Sender.Send(new Features.Users.Register.RegisterUserCommand
    {
        Email = Faker.Internet.Email(),
        Password = Faker.Internet.Password(),
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    }, TestContext.Current.CancellationToken);

    await DbContext.Database.ExecuteSqlRawAsync(
      """
      INSERT INTO users.user_permissions (user_id, permission_code)
      VALUES ({0}, 'users:read')
      """,
      userId.Value);

    var users = await DbContext.Users.ToListAsync(TestContext.Current.CancellationToken);

    var identityId = users.FirstOrDefault(x => x.Id.Value == userId.Value)!.IdentityId;

    // Act
    Result<PermissionsResponse> permissionsResult = await Sender.Send(new Features.Users.GetUserPermissions.GetUserPermissionsQuery(identityId), TestContext.Current.CancellationToken);

    // Assert
    permissionsResult.IsSuccess.Should().BeTrue();
    permissionsResult.Value.Permissions.Should().NotBeEmpty();
  }
}
