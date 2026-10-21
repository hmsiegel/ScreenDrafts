namespace ScreenDrafts.Modules.Administration.IntegrationTests.Users;

public sealed class ListUsersRoleFilterTests(AdministrationIntegrationTestWebAppFactory factory)
  : AdministrationIntegrationTest(factory)
{
  [Fact]
  public async Task ListUsers_WithRoleFilter_ShouldReturnOnlyUsersHoldingThatRoleAsync()
  {
    // Arrange
    var (hostId, hostPublicId) = await InsertUserAsync();
    var (_, noRolePublicId) = await InsertUserAsync();
    await InsertRoleAsync("Host");
    await AssignRoleAsync(hostId, "Host");

    // Act
    var result = await Sender.Send(
      new ListUsersQuery { Roles = ["Host"], Page = 1, PageSize = 50 },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Should().ContainSingle(u => u.PublicId == hostPublicId);
    result.Value.Items.Should().NotContain(u => u.PublicId == noRolePublicId);
  }

  [Fact]
  public async Task ListUsers_WithSeveralRoles_ShouldReturnUsersHoldingAnyOfThemAsync()
  {
    // Arrange
    var (hostId, hostPublicId) = await InsertUserAsync();
    var (drafterId, drafterPublicId) = await InsertUserAsync();
    var (guestId, guestPublicId) = await InsertUserAsync();
    await InsertRoleAsync("Host");
    await InsertRoleAsync("Drafter");
    await InsertRoleAsync("Guest");
    await AssignRoleAsync(hostId, "Host");
    await AssignRoleAsync(drafterId, "Drafter");
    await AssignRoleAsync(guestId, "Guest");

    // Act
    var result = await Sender.Send(
      new ListUsersQuery { Roles = ["Host", "Drafter"], Page = 1, PageSize = 50 },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(2);
    result.Value.Items.Select(u => u.PublicId)
      .Should()
      .BeEquivalentTo(hostPublicId, drafterPublicId);
    result.Value.Items.Should().NotContain(u => u.PublicId == guestPublicId);
  }

  [Fact]
  public async Task ListUsers_WithRoleFilter_ShouldKeepAllOfAUsersRolesInTheResponseAsync()
  {
    // Arrange
    var (userId, publicId) = await InsertUserAsync();
    await InsertRoleAsync("Host");
    await InsertRoleAsync("Drafter");
    await AssignRoleAsync(userId, "Host");
    await AssignRoleAsync(userId, "Drafter");

    // Act
    var result = await Sender.Send(
      new ListUsersQuery { Roles = ["Host"], Page = 1, PageSize = 50 },
      TestContext.Current.CancellationToken
    );

    // Assert -- filtering selects users; it must not trim the roles shown for them.
    result.Value.Items.Single(u => u.PublicId == publicId)
      .Roles.Should()
      .BeEquivalentTo("Host", "Drafter");
  }

  [Fact]
  public async Task ListUsers_WithRoleFilter_ShouldPageOverTheFilteredSetAsync()
  {
    // Arrange
    await InsertRoleAsync("Host");
    for (var i = 0; i < 3; i++)
    {
      var (id, _) = await InsertUserAsync();
      await AssignRoleAsync(id, "Host");
    }
    await InsertUserAsync(); // no role; must not count toward the total

    // Act
    var secondPage = await Sender.Send(
      new ListUsersQuery { Roles = ["Host"], Page = 2, PageSize = 2 },
      TestContext.Current.CancellationToken
    );

    // Assert
    secondPage.Value.TotalCount.Should().Be(3);
    secondPage.Value.Items.Should().HaveCount(1);
  }

  [Fact]
  public async Task ListUsers_WithRoleFilterAndSearch_ShouldApplyBothAsync()
  {
    // Arrange
    var (hostId, _) = await InsertUserAsync();
    await InsertRoleAsync("Host");
    await AssignRoleAsync(hostId, "Host");

    // Act -- a search term that matches no one wins over a matching role.
    var result = await Sender.Send(
      new ListUsersQuery
      {
        Search = "no-such-user-zzz",
        Roles = ["Host"],
        Page = 1,
        PageSize = 50,
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  private async Task AssignRoleAsync(Guid userId, string roleName)
  {
    var connectionFactory = GetService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync(
      TestContext.Current.CancellationToken
    );
    await connection.ExecuteAsync(
      "INSERT INTO administration.user_roles (user_id, role_name) VALUES (@UserId, @RoleName)",
      new { UserId = userId, RoleName = roleName }
    );
  }
}
