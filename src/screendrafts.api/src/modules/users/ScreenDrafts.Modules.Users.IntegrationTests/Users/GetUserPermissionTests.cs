using ScreenDrafts.Common.Features.Abstractions.Authorization;

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
    Result<PermissionsResponse> permissionsResult = await Sender.Send(new GetUserPermissionsQuery(identityId));

    // Assert
    permissionsResult.Errors[0].Should().Be(UserErrors.NotFound(identityId));
  }

  [Fact]
  public async Task Should_ReturnPermissions_WhenUserExistsAsync()
  {
    // Arrange
    Result<Guid> result = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));

    var users = await DbContext.Users.ToListAsync();

    var identityId = users.FirstOrDefault(x => x.Id.Value == result.Value)!.IdentityId;

    // Act
    Result<PermissionsResponse> permissionsResult = await Sender.Send(new GetUserPermissionsQuery(identityId));

    // Assert
    permissionsResult.IsSuccess.Should().BeTrue();
    permissionsResult.Value.Permissions.Should().NotBeEmpty();
  }
}
