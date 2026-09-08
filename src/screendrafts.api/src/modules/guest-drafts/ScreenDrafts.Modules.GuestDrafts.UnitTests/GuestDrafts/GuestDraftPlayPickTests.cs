using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftPlayPickTests : GuestDraftsBaseTest
{
  [Fact]
  public void PlayPick_ShouldReturnFailure_WhenStatusIsNotInProgress()
  {
    // Arrange -- board set up but never started
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    // Act
    var result = guestDraft.PlayPick(
      Faker.Random.AlphaNumeric(10),
      Guid.NewGuid(),
      7,
      1,
      owner.Id.Value
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DraftNotStarted);
  }

  [Fact]
  public void PlayPick_ShouldReturnFailure_WhenParticipantIsNotInTheDraft()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();
    var strangerId = Guid.NewGuid();

    // Act
    var result = guestDraft.PlayPick(
      Faker.Random.AlphaNumeric(10),
      Guid.NewGuid(),
      7,
      1,
      strangerId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.ParticipantNotFound(strangerId));
  }

  [Fact]
  public void PlayPick_ShouldReturnFailure_WhenTheSameMovieHasAlreadyBeenPickedAndTheEarlierPickHasNotBeenVetoed()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var moviePublicId = Faker.Random.AlphaNumeric(10);
    guestDraft.PlayPick(moviePublicId, Guid.NewGuid(), 7, 1, owner.Id.Value);

    // Act -- different position, same movie
    var result = guestDraft.PlayPick(moviePublicId, Guid.NewGuid(), 6, 2, owner.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.MovieAlreadyPicked);
  }

  [Fact]
  public void PlayPick_ShouldReturnFailure_WhenThePositionAlreadyHasALandedPick()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value);

    // Act -- same position, different movie
    var result = guestDraft.PlayPick(
      Faker.Random.AlphaNumeric(10),
      Guid.NewGuid(),
      7,
      2,
      owner.Id.Value
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PickPositionAlreadyExists(7));
  }

  [Fact]
  public void PlayPick_ShouldSucceed_WhenThePositionAndMovieWerePreviouslyVetoedAndNotOverridden()
  {
    // Arrange -- a vetoed, un-overridden pick is eligible for re-pick: it blocks
    // neither its position nor its movie.
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var moviePublicId = Faker.Random.AlphaNumeric(10);
    var firstPick = guestDraft.PlayPick(moviePublicId, Guid.NewGuid(), 7, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(firstPick, other.Id.Value);

    // Act -- same position, same movie
    var result = guestDraft.PlayPick(moviePublicId, Guid.NewGuid(), 7, 2, owner.Id.Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void PlayPick_ShouldAutoAssignTheOtherParticipantAsRevealer_WhenThereAreExactlyTwoParticipants()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();

    // Act
    var pickId = guestDraft
      .PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value)
      .Value;

    // Assert
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);
    pick.RevealAuthorizedParticipantId.Should().Be(other.Id);
  }

  [Fact]
  public void PlayPick_ShouldHonorExplicitRevealRecipientId_WhenThereAreMoreThanTwoParticipants()
  {
    // Arrange
    var (guestDraft, participants, _) = CreateInProgressCustomGuestDraft(3);
    var picker = participants[0];
    var explicitRecipient = participants[2];

    // Act
    var pickId = guestDraft
      .PlayPick(
        Faker.Random.AlphaNumeric(10),
        Guid.NewGuid(),
        1,
        1,
        picker.Id.Value,
        explicitRevealRecipientId: explicitRecipient.Id
      )
      .Value;

    // Assert
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);
    pick.RevealAuthorizedParticipantId.Should().Be(explicitRecipient.Id);
  }

  [Fact]
  public void PlayPick_ShouldNotAssignARevealer_WhenThereAreMoreThanTwoParticipantsAndNoExplicitRecipientIsGiven()
  {
    // Arrange -- with >2 participants the random draw is the caller's job; PlayPick
    // itself must not guess a revealer.
    var (guestDraft, participants, _) = CreateInProgressCustomGuestDraft(3);
    var picker = participants[0];

    // Act
    var pickId = guestDraft
      .PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 1, 1, picker.Id.Value)
      .Value;

    // Assert
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);
    pick.RevealAuthorizedParticipantId.Should().BeNull();
  }

  [Fact]
  public void UndoPick_ShouldRemoveThePick_WhenAPickWithThatPlayOrderExists()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value);

    // Act
    var result = guestDraft.UndoPick(1);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.Picks.Should().BeEmpty();
  }

  [Fact]
  public void UndoPick_ShouldReturnSuccess_WhenNoPickWithThatPlayOrderExists()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value);

    // Act
    var result = guestDraft.UndoPick(99);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.Picks.Should().HaveCount(1);
  }
}
