namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafters;

/// <summary>
/// UpdateDrafterNameCommand had no coverage at all before this file. It's only ever
/// invoked from UserNameUpdatedIntegrationEventConsumer, never from an HTTP endpoint,
/// but its find-or-create branch (a deliberate upsert -- see the handler's own
/// comment) is exactly the kind of branching logic this audit prioritizes: a caller
/// who was never seen before must get a fresh Drafter row rather than the update
/// being silently dropped.
/// </summary>
public sealed class UpdateDrafterNameTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task UpdateDrafterName_ForExistingDrafter_ShouldUpdateNameAsync()
  {
    // Arrange
    var user = await CreateUserAsync();

    // Act
    var result = await Sender.Send(
      new UpdateDrafterNameCommand
      {
        UserId = (await FakeUsersApi.GetUserByPublicId(user.UserPublicId, TestContext.Current.CancellationToken))!.UserId,
        FirstName = "Grace",
        LastName = "Hopper",
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var drafter = await DbContext.Drafters.FirstAsync(
      d => d.PublicId == user.GuestDrafterPublicId,
      TestContext.Current.CancellationToken
    );
    drafter.FirstName.Should().Be("Grace");
    drafter.LastName.Should().Be("Hopper");
  }

  [Fact]
  public async Task UpdateDrafterName_ForUserWithNoExistingDrafterRecord_ShouldCreateOneAsync()
  {
    // Arrange -- deliberate upsert: a user registered before this consumer existed
    // (or before GuestDrafts existed) has no Drafter row yet, and this is the only
    // event that will ever tell us their name again.
    var userId = Guid.NewGuid();

    // Act
    var result = await Sender.Send(
      new UpdateDrafterNameCommand
      {
        UserId = userId,
        FirstName = "Katherine",
        LastName = "Johnson",
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var drafter = await DbContext.Drafters.SingleAsync(
      d => d.UserId == userId,
      TestContext.Current.CancellationToken
    );
    drafter.FirstName.Should().Be("Katherine");
    drafter.LastName.Should().Be("Johnson");
  }

  [Fact]
  public async Task UpdateDrafterName_WithBlankFirstName_ShouldFailAsync()
  {
    // Arrange
    var user = await CreateUserAsync();
    var userId = (
      await FakeUsersApi.GetUserByPublicId(user.UserPublicId, TestContext.Current.CancellationToken)
    )!.UserId;

    // Act
    var result = await Sender.Send(
      new UpdateDrafterNameCommand
      {
        UserId = userId,
        FirstName = "   ",
        LastName = "Hopper",
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DrafterErrors.InvalidFirstName.Code);
  }
}
