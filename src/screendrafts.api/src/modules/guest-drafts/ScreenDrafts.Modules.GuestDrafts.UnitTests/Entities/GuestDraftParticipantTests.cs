namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Entities;

/// <summary>
/// GuestDraftParticipant's veto/override/fungible-token economy. The mutators
/// (InitializeVetoes, GrantAward, SpendVeto, RefundVeto, ...) are internal to the
/// aggregate, so state is driven through GuestDraft's public API; the assertions
/// below focus purely on the participant's own budget invariants rather than on
/// GuestDraft's orchestration (which is covered separately in the GuestDrafts folder).
/// </summary>
public class GuestDraftParticipantTests : GuestDraftsBaseTest
{
  [Fact]
  public void CanUseVeto_ShouldReturnTrue_WhenTheStartingVetoIsUnused()
  {
    // Arrange
    var (_, owner, _) = CreateInProgressStandardGuestDraft();

    // Act & Assert
    owner.CanUseVeto().Should().BeTrue();
  }

  [Fact]
  public void CanUseVeto_ShouldReturnFalse_WhenTheNormalPoolIsUsedUpAndNoFungibleTokensExist()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act & Assert
    other.CanUseVeto().Should().BeFalse();
  }

  [Fact]
  public void CanUseVeto_ShouldReturnTrue_WhenTheNormalPoolIsUsedUpButAFungibleTokenIsAvailable()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var owner = guestDraft.Participants.Single();
    var other = InviteParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], false, false, false),
      ("B", [2], false, false, true),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), owner.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), other.Id.Value);
    guestDraft.Start();

    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 1, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act & Assert -- normal pool now spent, but the awarded fungible token remains
    other.CanUseVeto().Should().BeTrue();
  }

  [Fact]
  public void CanUseVetoOverride_ShouldReturnFalse_WhenNoOverridesAreAwardedAndNoFungibleTokensExist()
  {
    // Arrange
    var (_, owner, _) = CreateInProgressStandardGuestDraft();

    // Act & Assert
    owner.CanUseVetoOverride(maxOverrides: 1).Should().BeFalse();
  }

  [Fact]
  public void CanUseVetoOverride_ShouldReturnFalse_WhenMaxOverridesIsZero_EvenWithAnAwardedOverride()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var owner = guestDraft.Participants.Single();
    var other = InviteParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], false, true, false),
      ("B", [2], false, false, false),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), owner.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), other.Id.Value);
    guestDraft.Start();

    // Act & Assert -- the awarded override is real, but a zero cap still blocks it
    owner.CanUseVetoOverride(maxOverrides: 0).Should().BeFalse();
  }

  [Fact]
  public void CanUseVetoOverride_ShouldReturnTrue_WhenAnOverrideIsAwardedAndMaxOverridesIsPositive()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var owner = guestDraft.Participants.Single();
    var other = InviteParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], false, true, false),
      ("B", [2], false, false, false),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), owner.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), other.Id.Value);
    guestDraft.Start();

    // Act & Assert
    owner.CanUseVetoOverride(maxOverrides: 1).Should().BeTrue();
  }

  [Fact]
  public void CanUseVetoOverride_ShouldReturnTrue_WhenAFungibleTokenIsAvailable_EvenIfMaxOverridesIsZero()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var owner = guestDraft.Participants.Single();
    var other = InviteParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], false, false, true),
      ("B", [2], false, false, false),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), owner.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), other.Id.Value);
    guestDraft.Start();

    // Act & Assert -- fungible-pool fallback is independent of maxOverrides entirely
    owner.CanUseVetoOverride(maxOverrides: 0).Should().BeTrue();
  }

  [Fact]
  public void TotalVetoes_ShouldBeStartingPlusAwarded()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var owner = guestDraft.Participants.Single();
    var other = InviteParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], true, false, false),
      ("B", [2], false, false, false),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), owner.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), other.Id.Value);

    // Act
    guestDraft.Start();

    // Assert -- 1 starting (from Start) + 1 awarded (from the bonus position)
    owner.TotalVetoes.Should().Be(2);
  }
}
