namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftRevealPickTests : GuestDraftsBaseTest
{
  [Fact]
  public void RevealPick_ShouldSucceed_WhenPickHasNotBeenRevealed()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;

    // Act
    var result = guestDraft.RevealPick(pickId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.Picks.Single(p => p.Id == pickId).IsRevealed.Should().BeTrue();
  }

  [Fact]
  public void RevealPick_ShouldReturnFailure_WhenPickIsAlreadyRevealed()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    guestDraft.RevealPick(pickId);

    // Act
    var result = guestDraft.RevealPick(pickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickAlreadyRevealed);
  }

  [Fact]
  public void RevealPick_ShouldReturnFailure_WhenPickDoesNotExist()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();
    var missingPickId = GuestDraftPickId.CreateUnique();

    // Act
    var result = guestDraft.RevealPick(missingPickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickNotFound(missingPickId.Value));
  }

  [Fact]
  public void RevealPick_ShouldReturnFailure_WhenStatusIsNotInProgress()
  {
    // Arrange -- complete the draft, then try to reveal one of its landed picks
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];
    var pickIds = pickSlots
      .Select((slot, index) => guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), slot, index + 1, owner.Id.Value).Value)
      .ToList();
    guestDraft.Complete();

    // Act
    var result = guestDraft.RevealPick(pickIds[0]);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.DraftNotStarted);
  }

  [Fact]
  public void IsRevealAuthorized_ShouldReturnTrue_OnlyForTheAssignedRevealer()
  {
    // Arrange -- exactly 2 participants, so the other participant auto-assigns
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);

    // Act & Assert
    pick.IsRevealAuthorized(other.Id.Value).Should().BeTrue();
  }

  [Fact]
  public void IsRevealAuthorized_ShouldReturnFalse_ForThePickerThemselves()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);

    // Act & Assert
    pick.IsRevealAuthorized(owner.Id.Value).Should().BeFalse();
  }

  [Fact]
  public void IsRevealAuthorized_ShouldReturnFalse_ForAnUnrelatedParticipant()
  {
    // Arrange
    var (guestDraft, participants, _) = CreateInProgressCustomGuestDraft(3);
    var picker = participants[0];
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 1, 1, picker.Id.Value).Value;
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);

    // Act & Assert -- no explicit recipient was given, so nobody is authorized
    pick.IsRevealAuthorized(participants[1].Id.Value).Should().BeFalse();
    pick.IsRevealAuthorized(participants[2].Id.Value).Should().BeFalse();
  }
}
