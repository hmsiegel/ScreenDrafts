namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftApplyVetoOverrideTests : GuestDraftsBaseTest
{
  [Fact]
  public void ApplyVetoOverride_ShouldReturnFailure_ForStandardDraftType()
  {
    // Arrange -- Standard blocks overrides outright, before anything else is checked
    var guestDraft = CreateGuestDraft(GuestDraftType.Standard);

    // Act
    var result = guestDraft.ApplyVetoOverride(GuestDraftPickId.CreateUnique(), Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.VetoOverridesNotAllowedForThisDraftType);
  }

  [Fact]
  public void ApplyVetoOverride_ShouldReturnFailure_WhenStatusIsNotInProgress()
  {
    // Arrange
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    AddParticipant(guestDraft);

    // Act
    var result = guestDraft.ApplyVetoOverride(GuestDraftPickId.CreateUnique(), Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.DraftNotStarted);
  }

  [Fact]
  public void ApplyVetoOverride_ShouldReturnFailure_WhenPickDoesNotExist()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressMiniMegaDraft();
    var missingPickId = GuestDraftPickId.CreateUnique();

    // Act
    var result = guestDraft.ApplyVetoOverride(missingPickId, Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickNotFound(missingPickId.Value));
  }

  [Fact]
  public void ApplyVetoOverride_ShouldReturnFailure_WhenThePickHasNoActiveVeto()
  {
    // Arrange
    var (guestDraft, picker, other) = CreateInProgressMiniMegaDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 1, 1, picker.Id.Value).Value;

    // Act
    var result = guestDraft.ApplyVetoOverride(pickId, other.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.VetoNotFound(pickId.Value));
  }

  [Fact]
  public void ApplyVetoOverride_ShouldReturnFailure_WhenTheVetoIsAlreadyOverridden()
  {
    // Arrange -- two participants, each with their own override budget, so the
    // second attempt reaches the "already overridden" check on its own merits
    // rather than tripping the budget check first.
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var picker = AddParticipant(guestDraft, isOwner: true);
    var firstOverrider = AddParticipant(guestDraft);
    var secondOverrider = AddParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], false, false, false),
      ("B", [2], false, true, false),
      ("C", [3], false, true, false),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), picker.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), firstOverrider.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "C"), secondOverrider.Id.Value);
    guestDraft.Start();

    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 1, 1, picker.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, firstOverrider.Id.Value);
    guestDraft.ApplyVetoOverride(pickId, firstOverrider.Id.Value);

    // Act
    var result = guestDraft.ApplyVetoOverride(pickId, secondOverrider.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.VetoOverrideAlreadyUsed);
  }

  [Fact]
  public void ApplyVetoOverride_ShouldReturnFailure_WhenTheParticipantTriesToOverrideTheVetoOnTheirOwnPick()
  {
    // Arrange
    var (guestDraft, picker, other) = CreateInProgressMiniMegaDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 1, 1, picker.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act
    var result = guestDraft.ApplyVetoOverride(pickId, picker.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.CannotOverrideOwnPick);
  }

  [Fact]
  public void ApplyVetoOverride_ShouldReturnFailure_WhenParticipantHasNoRemainingOverridesOrFungibleTokens()
  {
    // Arrange -- "other" is not awarded any override budget
    var (guestDraft, picker, other) = CreateInProgressMiniMegaDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 1, 1, picker.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act
    var result = guestDraft.ApplyVetoOverride(pickId, other.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.NoRemainingVetoOverrides);
  }

  [Fact]
  public void ApplyVetoOverride_ShouldSucceed_WhenParticipantHasAnAwardedOverride()
  {
    // Arrange
    var (guestDraft, picker, other) = CreateInProgressMiniMegaDraft(otherHasBonusOverride: true);
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 1, 1, picker.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act
    var result = guestDraft.ApplyVetoOverride(pickId, other.Id.Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
    other.VetoOverridesUsed.Should().Be(1);
    guestDraft.Picks.Single(p => p.Id == pickId).CurrentVeto!.IsOverridden.Should().BeTrue();
  }

  [Fact]
  public void ApplyVetoOverride_ShouldSpendFromTheFungiblePool_WhenTheNormalOverridePoolIsExhaustedButAFungibleTokenExists()
  {
    // Arrange -- exhaust "other"'s one awarded override on a first pick, then have
    // the picker veto their own second pick so "other"'s fungible token (not their
    // veto pool) is the only thing left to pay for the second override with.
    var (guestDraft, picker, other) = CreateInProgressMiniMegaDraft(
      otherHasBonusOverride: true,
      otherHasBonusFungibleToken: true);

    var firstPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 1, 1, picker.Id.Value).Value;
    guestDraft.ApplyVeto(firstPickId, other.Id.Value);
    guestDraft.ApplyVetoOverride(firstPickId, other.Id.Value);

    var secondPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 2, 2, picker.Id.Value).Value;
    guestDraft.ApplyVeto(secondPickId, picker.Id.Value);

    // Act
    var result = guestDraft.ApplyVetoOverride(secondPickId, other.Id.Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
    other.VetoOverridesUsed.Should().Be(1, "the normal override pool was already exhausted and must stay untouched");
    other.FungibleTokensUsed.Should().Be(1);
    guestDraft.Picks.Single(p => p.Id == secondPickId).CurrentVeto!.VetoOverride!.SpentFromFungiblePool.Should().BeTrue();
  }

  /// <summary>
  /// Two-participant MiniMega draft: picker holds position "A" ([1]), "other"
  /// holds position "B" ([2]), optionally with bonus veto-override / fungible-token
  /// awards, started and ready for picks.
  /// </summary>
  private static (GuestDraft GuestDraft, GuestDraftParticipant Picker, GuestDraftParticipant Other) CreateInProgressMiniMegaDraft(
    bool otherHasBonusOverride = false,
    bool otherHasBonusFungibleToken = false)
  {
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var picker = AddParticipant(guestDraft, isOwner: true);
    var other = AddParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], false, false, false),
      ("B", [2], false, otherHasBonusOverride, otherHasBonusFungibleToken),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);

    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), picker.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), other.Id.Value);
    guestDraft.Start();

    return (guestDraft, picker, other);
  }
}
