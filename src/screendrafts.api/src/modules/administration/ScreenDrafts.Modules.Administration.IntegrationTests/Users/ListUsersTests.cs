namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

/// <summary>
/// ListUsersQueryHandler had no coverage at all before this file. Unlike the
/// module's other read handlers (thin single-table SELECTs), this one fetches every
/// user cross-module via IUsersApi, paginates in memory, then joins role names back
/// in from administration.user_roles by internal user id -- real assembly logic
/// worth pinning down, especially the role-association join and page-boundary math.
/// </summary>
public sealed class ListUsersTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task ListUsers_ShouldAttachRolesToTheMatchingUserOnlyAsync()
  {
    // Arrange
    var (userIdWithRole, publicIdWithRole) = await InsertUserAsync();
    var (_, publicIdWithoutRole) = await InsertUserAsync();
    const string roleName = "Host";
    await InsertRoleAsync(roleName);
    var connectionFactory = GetService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync(
      TestContext.Current.CancellationToken
    );
    await connection.ExecuteAsync(
      "INSERT INTO administration.user_roles (user_id, role_name) VALUES (@UserId, @RoleName)",
      new { UserId = userIdWithRole, RoleName = roleName }
    );

    // Act
    var result = await Sender.Send(
      new ListUsersQuery { Page = 1, PageSize = 50 },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var withRole = result.Value.Items.Single(u => u.PublicId == publicIdWithRole);
    withRole.Roles.Should().ContainSingle(r => r == roleName);
    var withoutRole = result.Value.Items.Single(u => u.PublicId == publicIdWithoutRole);
    withoutRole.Roles.Should().BeEmpty();
  }

  [Fact]
  public async Task ListUsers_ShouldAssembleDisplayNameFromFirstAndLastNameAsync()
  {
    // Arrange
    var (_, publicId) = await InsertUserAsync();
    var expected = (await Sender.Send(
      new ListUsersQuery { Page = 1, PageSize = 50 },
      TestContext.Current.CancellationToken
    )).Value.Items.Single(u => u.PublicId == publicId);

    // Assert -- DisplayName is "{FirstName} {LastName}".Trim(), not empty or just one part
    expected.DisplayName.Should().NotBeNullOrWhiteSpace();
    expected.DisplayName.Should().Contain(" ");
  }

  [Fact]
  public async Task ListUsers_ShouldRespectPageSizeAndPageAsync()
  {
    // Arrange
    for (var i = 0; i < 3; i++)
    {
      await InsertUserAsync();
    }

    // Act
    var firstPage = await Sender.Send(
      new ListUsersQuery { Page = 1, PageSize = 2 },
      TestContext.Current.CancellationToken
    );
    var secondPage = await Sender.Send(
      new ListUsersQuery { Page = 2, PageSize = 2 },
      TestContext.Current.CancellationToken
    );

    // Assert
    firstPage.IsSuccess.Should().BeTrue();
    secondPage.IsSuccess.Should().BeTrue();
    firstPage.Value.Items.Should().HaveCount(2);
    firstPage.Value.TotalCount.Should().Be(3);
    secondPage.Value.Items.Should().HaveCount(1);
    firstPage
      .Value.Items.Select(i => i.PublicId)
      .Should()
      .NotIntersectWith(secondPage.Value.Items.Select(i => i.PublicId));
  }

  [Fact]
  public async Task ListUsers_WithNoUsers_ShouldReturnAnEmptyPageAsync()
  {
    // Arrange, Act -- exercises the handler's empty-page short-circuit branch, which
    // skips the role query entirely.
    var result = await Sender.Send(
      new ListUsersQuery { Page = 1, PageSize = 50 },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }
}
