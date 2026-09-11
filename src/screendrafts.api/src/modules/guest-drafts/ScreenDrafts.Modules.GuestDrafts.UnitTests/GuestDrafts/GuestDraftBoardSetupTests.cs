using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftBoardSetupTests : GuestDraftsBaseTest
{
  // ── UseFixedBoardLayout ────────────────────────────────────────────────────

  [Fact]
  public void UseFixedBoardLayout_ShouldApplyStandardTemplateExactly_ForStandardDraftType()
  {
    // Arrange -- the fixed Standard template always produces 2 positions, so the
    // board needs exactly 2 participants at apply time.
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);

    // Act
    var result = guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.GameBoard.Should().NotBeNull();
    var positions = guestDraft.GameBoard.Positions;
    positions.Should().HaveCount(2);
    positions.Single(p => p.Name == "A").Picks.Should().BeEquivalentTo([7, 6, 4, 2]);
    positions.Single(p => p.Name == "B").Picks.Should().BeEquivalentTo([5, 3, 1]);
  }

  [Fact]
  public void UseFixedBoardLayout_ShouldApplyMiniSuperTemplateExactly_ForMiniSuperDraftType()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.MiniSuper);
    AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);

    // Act
    var result = guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var positions = guestDraft.GameBoard!.Positions;
    positions.Should().HaveCount(2);
    positions.Single(p => p.Name == "A").Picks.Should().BeEquivalentTo([5, 3, 1]);
    positions.Single(p => p.Name == "B").Picks.Should().BeEquivalentTo([4, 2]);
  }

  [Theory]
  [MemberData(nameof(NonFixedDraftTypes))]
  public void UseFixedBoardLayout_ShouldReturnFailure_ForNonFixedDraftTypes(
    DraftType guestDraftType
  )
  {
    ArgumentNullException.ThrowIfNull(guestDraftType);

    // Arrange -- fails on the type check before participant count is ever consulted.
    var guestDraft = CreateGuestDraft(guestDraftType);
    AddParticipant(guestDraft);

    // Act
    var result = guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DraftTypeDoesNotHaveAFixedLayout(guestDraftType.Name));
    guestDraft.GameBoard.Should().BeNull();
  }

  [Fact]
  public void UseFixedBoardLayout_ShouldReturnFailure_WhenStatusIsNotCreated()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();

    // Act
    var result = guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.CannotChangeBoardAfterStart);
  }

  public static TheoryData<DraftType> NonFixedDraftTypes() =>
    new() { DraftType.MiniMega, DraftType.Super, DraftType.Mega };

  [Fact]
  public void UseFixedBoardLayout_ShouldSucceed_WhenCalledImmediatelyAfterCreateWithZeroParticipants()
  {
    // Arrange -- regression test: AssignPositions used to require positions.Count ==
    // participantCount, which was impossible to satisfy at board-setup time now that
    // Create no longer auto-adds any participants. The participantCount parameter
    // was removed entirely; board setup must succeed with zero participants added.
    var guestDraft = CreateGuestDraft(DraftType.Standard);

    // Act
    var result = guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.Participants.Should().BeEmpty();
    guestDraft.GameBoard!.Positions.Should().HaveCount(2);
  }

  // ── SetCustomPositions ───────────────────────────────────────────────────

  [Fact]
  public void SetCustomPositions_ShouldSucceed_WhenValidPositionsAreProvidedForACustomDraftType()
  {
    // Arrange -- 2 positions supplied below, so the board needs exactly 2
    // participants at apply time.
    var guestDraft = CreateGuestDraft(DraftType.MiniMega);
    AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);
    var positions = TwoCustomPositions();

    // Act
    var result = guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.GameBoard!.Positions.Should().HaveCount(2);
  }

  [Theory]
  [MemberData(nameof(FixedDraftTypes))]
  public void SetCustomPositions_ShouldReturnFailure_ForFixedDraftTypes(DraftType guestDraftType)
  {
    ArgumentNullException.ThrowIfNull(guestDraftType);

    // Arrange -- fails on the fixed-layout check before participant count is ever
    // consulted.
    var guestDraft = CreateGuestDraft(guestDraftType);
    AddParticipant(guestDraft);
    var positions = TwoCustomPositions();

    // Act
    var result = guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DraftTypeHasAFixedLayout(guestDraftType.Name));
  }

  public static TheoryData<DraftType> FixedDraftTypes() =>
    new() { DraftType.Standard, DraftType.MiniSuper };

  [Fact]
  public void SetCustomPositions_ShouldReturnFailure_WhenStatusIsNotCreated()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressCustomGuestDraft(2);

    // Act
    var result = guestDraft.SetCustomPositions(TwoCustomPositions(), GeneratePositionPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.CannotChangeBoardAfterStart);
  }

  [Fact]
  public void SetCustomPositions_ShouldSucceed_WhenCalledImmediatelyAfterCreateWithZeroParticipants()
  {
    // Arrange -- regression test: AssignPositions used to require positions.Count ==
    // participantCount, which was impossible to satisfy at board-setup time now that
    // Create no longer auto-adds any participants. The participantCount parameter
    // was removed entirely; board setup must succeed with zero participants added.
    var guestDraft = CreateGuestDraft(DraftType.MiniMega);

    // Act
    var result = guestDraft.SetCustomPositions(TwoCustomPositions(), GeneratePositionPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.Participants.Should().BeEmpty();
    guestDraft.GameBoard!.Positions.Should().HaveCount(2);
  }

  [Fact]
  public void SetCustomPositions_ShouldReturnFailure_WhenPickSlotsAreDuplicatedAcrossPositions()
  {
    // Arrange -- 2 positions supplied below; participant count must match (2) for
    // the duplicate-pick-slots check to be reached at all, rather than tripping
    // InvalidNumberOfPositions first.
    var guestDraft = CreateGuestDraft(DraftType.MiniMega);
    AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);

    List<(
      string Name,
      IReadOnlyList<int> Picks,
      bool HasBonusVeto,
      bool HasBonusVetoOverride,
      bool HasBonusFungibleToken
    )> positions = [("A", [1], false, false, false), ("B", [1], false, false, false)];

    // Act
    var result = guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DuplicatePickSlots);
  }

  // ── AssignParticipantToPosition ──────────────────────────────────────────

  [Fact]
  public void AssignParticipantToPosition_ShouldSucceed_WhenPositionIsUnassignedAndBelongsToTheBoard()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var position = guestDraft.GameBoard!.Positions.First();

    // Act
    var result = guestDraft.AssignParticipantToPosition(position, owner.Id.Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
    position.AssignedToParticipantId.Should().Be(owner.Id.Value);
  }

  [Fact]
  public void AssignParticipantToPosition_ShouldReturnFailure_WhenPositionIsAlreadyAssigned()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    var other = AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    guestDraft.AssignParticipantToPosition(position, owner.Id.Value);

    // Act
    var result = guestDraft.AssignParticipantToPosition(position, other.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PositionAlreadyAssigned);
  }

  [Fact]
  public void AssignParticipantToPosition_ShouldReturnFailure_WhenPositionBelongsToADifferentBoard()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    var otherGuestDraft = CreateGuestDraft(DraftType.Standard);
    AddParticipant(otherGuestDraft, isOwner: true);
    AddParticipant(otherGuestDraft);
    otherGuestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var foreignPosition = otherGuestDraft.GameBoard!.Positions.First();

    // Act
    var result = guestDraft.AssignParticipantToPosition(foreignPosition, owner.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PositionDoesNotBelongToThisBoard);
  }

  [Fact]
  public void AssignParticipantToPosition_ShouldReturnFailure_WhenParticipantIsNotInTheDraft()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);
    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    var strangerId = Guid.NewGuid();

    // Act
    var result = guestDraft.AssignParticipantToPosition(position, strangerId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.ParticipantNotFound(strangerId));
  }

  [Fact]
  public void AssignParticipantToPosition_ShouldGrantBonusVeto_WhenPositionHasBonusVeto()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.MiniMega);
    var owner = AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);

    List<(
      string Name,
      IReadOnlyList<int> Picks,
      bool HasBonusVeto,
      bool HasBonusVetoOverride,
      bool HasBonusFungibleToken
    )> positions = [("A", [1], true, false, false), ("B", [2], false, false, false)];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");

    // Act
    guestDraft.AssignParticipantToPosition(positionA, owner.Id.Value);

    // Assert
    owner.AwardedVetoes.Should().Be(1);
    owner.AwardedVetoOverrides.Should().Be(0);
    owner.AwardedFungibleTokens.Should().Be(0);
  }

  [Fact]
  public void AssignParticipantToPosition_ShouldGrantBonusVetoOverride_WhenPositionHasBonusVetoOverride()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.MiniMega);
    var owner = AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);

    List<(
      string Name,
      IReadOnlyList<int> Picks,
      bool HasBonusVeto,
      bool HasBonusVetoOverride,
      bool HasBonusFungibleToken
    )> positions = [("A", [1], false, true, false), ("B", [2], false, false, false)];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");

    // Act
    guestDraft.AssignParticipantToPosition(positionA, owner.Id.Value);

    // Assert
    owner.AwardedVetoOverrides.Should().Be(1);
    owner.AwardedVetoes.Should().Be(0);
    owner.AwardedFungibleTokens.Should().Be(0);
  }

  [Fact]
  public void AssignParticipantToPosition_ShouldGrantBonusFungibleToken_WhenPositionHasBonusFungibleToken()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(DraftType.MiniMega);
    var owner = AddParticipant(guestDraft, isOwner: true);
    AddParticipant(guestDraft);

    List<(
      string Name,
      IReadOnlyList<int> Picks,
      bool HasBonusVeto,
      bool HasBonusVetoOverride,
      bool HasBonusFungibleToken
    )> positions = [("A", [1], false, false, true), ("B", [2], false, false, false)];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");

    // Act
    guestDraft.AssignParticipantToPosition(positionA, owner.Id.Value);

    // Assert
    owner.AwardedFungibleTokens.Should().Be(1);
    owner.AwardedVetoes.Should().Be(0);
    owner.AwardedVetoOverrides.Should().Be(0);
  }

  private static List<(
    string Name,
    IReadOnlyList<int> Picks,
    bool HasBonusVeto,
    bool HasBonusVetoOverride,
    bool HasBonusFungibleToken
  )> TwoCustomPositions() => [("A", [1], false, false, false), ("B", [2], false, false, false)];
}
