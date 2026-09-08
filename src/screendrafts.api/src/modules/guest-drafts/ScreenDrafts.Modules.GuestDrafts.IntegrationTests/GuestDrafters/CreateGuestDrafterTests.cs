using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafters;

/// <summary>
/// GuestDrafter itself has no dedicated unit-test coverage yet, and its one-per-user
/// constraint is enforced at the DB level (a unique index on UserId, see
/// GuestDrafterConfiguration) in addition to the command-level find-or-create check
/// -- something only an integration test against a real database can actually
/// exercise. Every other GuestDrafts integration test relies on CreateUserAsync to
/// set this up implicitly; these tests cover CreateGuestDrafterCommand directly.
/// </summary>
public sealed class CreateGuestDrafterTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateGuestDrafter_WithValidData_ShouldReturnPublicIdAndPersistAsync()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var command = new CreateGuestDrafterCommand
    {
      UserId = userId,
      FirstName = "Ada",
      LastName = "Lovelace",
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
    var guestDrafter = await DbContext.GuestDrafters.FirstAsync(
      d => d.PublicId == result.Value,
      TestContext.Current.CancellationToken
    );
    guestDrafter.UserId.Should().Be(userId);
    guestDrafter.DisplayName.Should().Be("Ada Lovelace");
  }

  [Fact]
  public async Task CreateGuestDrafter_WhenOneAlreadyExistsForTheSameUser_ShouldFailAsync()
  {
    // Arrange
    var userId = Guid.NewGuid();
    await Sender.Send(
      new CreateGuestDrafterCommand
      {
        UserId = userId,
        FirstName = "Ada",
        LastName = "Lovelace",
      },
      TestContext.Current.CancellationToken
    );

    // Act
    var result = await Sender.Send(
      new CreateGuestDrafterCommand
      {
        UserId = userId,
        FirstName = "Ada",
        LastName = "Lovelace",
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DrafterErrors.AlreadyExistsForUser(userId).Code);
  }
}
