using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftLifecycleTests : GuestDraftsBaseTest
{
  // ── Start ────────────────────────────────────────────────────────────────

  [Fact]
  public void Start_ShouldReturnFailure_WhenStatusIsNotCreated()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();

    // Act
    var result = guestDraft.Start();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DraftCanOnlyBeStartedIfCreated);
  }

  [Fact]
  public void Start_ShouldReturnFailure_WhenFewerThanTwoParticipants()
  {
    // Arrange -- nobody added at all
    var guestDraft = CreateGuestDraft();

    // Act
    var result = guestDraft.Start();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.CannotStartWithoutAtLeastTwoParticipants);
  }

  [Fact]
  public void Start_ShouldReturnFailure_WhenBoardHasNotBeenSetUp()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);

    // Act
    var result = guestDraft.Start();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.BoardMustBeFullySetUpBeforeStarting);
  }

  [Fact]
  public void Start_ShouldReturnFailure_WhenPositionCountDoesNotMatchParticipantCount()
  {
    // Arrange -- Standard's fixed layout always has 2 positions, but a 3rd
    // participant is added after the board is set up.
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    AddParticipant(guestDraft);

    // Act
    var result = guestDraft.Start();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.BoardMustBeFullySetUpBeforeStarting);
  }

  [Fact]
  public void Start_ShouldReturnFailure_WhenNotAllPositionsAreAssigned()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    guestDraft.AssignParticipantToPosition(position, owner.Id.Value);

    // Act
    var result = guestDraft.Start();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.AllPositionsMustBeAssignedBeforeStarting);
  }

  [Fact]
  public void Start_ShouldSucceedAndInitializeVetoBudgets_WhenParticipantsAndBoardAreValid()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    var other = AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var positions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(positions[0], owner.Id.Value);
    guestDraft.AssignParticipantToPosition(positions[1], other.Id.Value);

    // Act
    var result = guestDraft.Start();

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.GuestDraftStatus.Should().Be(DraftStatus.InProgress);
    owner.StartingVetoes.Should().Be(1);
    other.StartingVetoes.Should().Be(1);
    owner.CanUseVeto().Should().BeTrue();
    other.CanUseVeto().Should().BeTrue();
  }

  // ── Complete ─────────────────────────────────────────────────────────────

  [Fact]
  public void Complete_ShouldReturnFailure_WhenStatusIsNotInProgress()
  {
    // Arrange -- still Created, never started
    var guestDraft = CreateGuestDraft();
    AddParticipant(guestDraft);

    // Act
    var result = guestDraft.Complete();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.CannotCompleteIfNotInProgress);
  }

  [Fact]
  public void Complete_ShouldReturnFailure_WhenNotEveryBoardPositionHasALandedPick()
  {
    // Arrange -- the Standard board has seven pick slots across its two positions
    // in total; only one of them has landed here.
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value);

    // Act
    var result = guestDraft.Complete();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.CannotCompleteWithoutAllPicks);
  }

  [Fact]
  public void Complete_ShouldSucceed_WhenEveryBoardPositionHasALandedPick()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];

    for (var i = 0; i < pickSlots.Length; i++)
    {
      guestDraft.PlayPick(
        Faker.Random.AlphaNumeric(10),
        Guid.NewGuid(),
        pickSlots[i],
        i + 1,
        owner.Id.Value
      );
    }

    // Act
    var result = guestDraft.Complete();

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.GuestDraftStatus.Should().Be(DraftStatus.Completed);
  }
}
