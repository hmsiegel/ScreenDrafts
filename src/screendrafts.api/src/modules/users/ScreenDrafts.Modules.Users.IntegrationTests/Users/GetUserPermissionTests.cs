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
    Result<PermissionsResponse> permissionsResult = await Sender.Send(new Features.Users.GetUserPermissions.GetUserPermissionsQuery(identityId));

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
    });

    var users = await DbContext.Users.ToListAsync();

    var identityId = users.FirstOrDefault(x => x.Id.Value == userId.Value)!.IdentityId;

    // Act
    Result<PermissionsResponse> permissionsResult = await Sender.Send(new Features.Users.GetUserPermissions.GetUserPermissionsQuery(identityId));

    // Assert
    permissionsResult.IsSuccess.Should().BeTrue();
    permissionsResult.Value.Permissions.Should().NotBeEmpty();
  }
}
