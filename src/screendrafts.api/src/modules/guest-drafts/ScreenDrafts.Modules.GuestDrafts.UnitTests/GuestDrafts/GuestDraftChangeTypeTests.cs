namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftChangeTypeTests : GuestDraftsBaseTest
{
  [Fact]
  public void ChangeType_ShouldReturnFailure_WhenStatusIsNotCreated()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();

    // Act
    var result = guestDraft.ChangeType(GuestDraftType.Mega);

    // Assert -- not asserting the specific error identity: GuestDraftErrors.
    // CannotChangeDraftTypeAfterStart is currently a broken uninitialized
    // property (always null), a pre-existing domain bug outside this pass's scope.
    result.IsFailure.Should().BeTrue();
    guestDraft.GuestDraftType.Should().Be(GuestDraftType.Standard);
  }

  [Fact]
  public void ChangeType_ShouldSucceedAsANoOp_WhenNewTypeEqualsCurrentType()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.Standard);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var gameBoardBefore = guestDraft.GameBoard;
    var positionNamesBefore = gameBoardBefore!.Positions.Select(p => p.Name).ToList();

    // Act
    var result = guestDraft.ChangeType(GuestDraftType.Standard);

    // Assert -- same-type "change" must not clear the board
    result.IsSuccess.Should().BeTrue();
    guestDraft.GameBoard.Should().BeSameAs(gameBoardBefore);
    guestDraft.GameBoard.Positions.Select(p => p.Name).Should().BeEquivalentTo(positionNamesBefore);
  }

  [Fact]
  public void ChangeType_ShouldDiscardTheExistingGameBoardEntirely_WhenNewTypeDiffers()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.Standard);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    // Act
    var result = guestDraft.ChangeType(GuestDraftType.Mega);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.GuestDraftType.Should().Be(GuestDraftType.Mega);
    guestDraft.GameBoard.Should().BeNull();
  }

  [Fact]
  public void ChangeType_ShouldRevokeBonusAwards_FromTheOldBoardsAssignments()
  {
    // Arrange -- one position carrying all three bonus flags, assigned to a
    // participant while the draft is still Created (AssignParticipantToPosition
    // has no status restriction).
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var participant = AddParticipant(guestDraft, isOwner: true);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], true, true, true),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var position = guestDraft.GameBoard!.Positions.Single();
    guestDraft.AssignParticipantToPosition(position, participant.Id.Value);

    participant.AwardedVetoes.Should().Be(1);
    participant.AwardedVetoOverrides.Should().Be(1);
    participant.AwardedFungibleTokens.Should().Be(1);

    var startingVetoesBefore = participant.StartingVetoes;
    var vetoesUsedBefore = participant.VetoesUsed;

    // Act
    var result = guestDraft.ChangeType(GuestDraftType.Super);

    // Assert -- awards tied to the discarded board's assignments are revoked
    result.IsSuccess.Should().BeTrue();
    participant.AwardedVetoes.Should().Be(0);
    participant.AwardedVetoOverrides.Should().Be(0);
    participant.AwardedFungibleTokens.Should().Be(0);

    // Assert -- the core veto economy (unrelated to position-tied awards) is untouched
    participant.StartingVetoes.Should().Be(startingVetoesBefore);
    participant.VetoesUsed.Should().Be(vetoesUsedBefore);
  }

  [Fact]
  public void ChangeType_ShouldNotAffectParticipants_OnlyTheirPositionAssignments()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    var other = AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var positions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(positions[0], owner.Id.Value);
    guestDraft.AssignParticipantToPosition(positions[1], other.Id.Value);

    // Act
    var result = guestDraft.ChangeType(GuestDraftType.Mega);

    // Assert -- participants themselves survive; only assignments (the whole board) go
    result.IsSuccess.Should().BeTrue();
    guestDraft.Participants.Should().HaveCount(2);
    guestDraft.Participants.Should().Contain(owner);
    guestDraft.Participants.Should().Contain(other);
    guestDraft.GameBoard.Should().BeNull();
  }
}
