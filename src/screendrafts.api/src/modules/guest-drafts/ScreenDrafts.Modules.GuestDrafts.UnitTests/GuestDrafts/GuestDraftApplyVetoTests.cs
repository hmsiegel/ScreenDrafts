namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftApplyVetoTests : GuestDraftsBaseTest
{
  // ── ApplyVeto: veto scope guard ──────────────────────────────────────────

  [Fact]
  public void ApplyVeto_ShouldReturnFailure_WhenPickIsNotTheMostRecentlyPlayedPick()
  {
    // Arrange -- the older pick is untouched (unvetoed), but that alone doesn't
    // make it eligible: only the most recent pick, by play order, can be vetoed.
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var olderPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 6, 2, owner.Id.Value);

    // Act
    var result = guestDraft.ApplyVeto(olderPickId, other.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.VetoNotOnMostRecentPick);
  }

  [Fact]
  public void ApplyVeto_ShouldSucceed_WhenPickIsTheMostRecentlyPlayedPick()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;

    // Act
    var result = guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
    other.VetoesUsed.Should().Be(1);
    guestDraft.Picks.Single(p => p.Id == pickId).IsVetoed.Should().BeTrue();
  }

  // ── ApplyVeto: budget ────────────────────────────────────────────────────

  [Fact]
  public void ApplyVeto_ShouldReturnFailure_WhenParticipantHasNoRemainingVetoesAndNoFungibleTokens()
  {
    // Arrange -- spend the participant's one starting veto, then try again on the
    // (now re-pickable) most recent pick.
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var firstPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(firstPickId, other.Id.Value);
    var secondPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 2, owner.Id.Value).Value;

    // Act
    var result = guestDraft.ApplyVeto(secondPickId, other.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.NoRemainingVetoes);
  }

  [Fact]
  public void ApplyVeto_ShouldSpendFromTheFungiblePool_WhenTheNormalPoolIsExhaustedButAFungibleTokenExists()
  {
    // Arrange
    var (guestDraft, _, other, pickId) = CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleToken();

    // Act
    var result = guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
    other.VetoesUsed.Should().Be(1, "the normal pool was already exhausted and must stay untouched");
    other.FungibleTokensUsed.Should().Be(1);
    guestDraft.Picks.Single(p => p.Id == pickId).CurrentVeto!.SpentFromFungiblePool.Should().BeTrue();
  }

  [Fact]
  public void ApplyVeto_ShouldReturnFailure_WhenThePickIsAlreadyVetoed()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act -- owner still has their full veto budget; the pick is simply already vetoed
    var result = guestDraft.ApplyVeto(pickId, owner.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickAlreadyVetoed);
    owner.VetoesUsed.Should().Be(0, "the spent veto must be refunded when applying it to the pick fails");
  }

  [Fact]
  public void ApplyVeto_ShouldReturnFailure_WhenStatusIsNotInProgress()
  {
    // Arrange -- complete the draft, then try to veto its last (and therefore most
    // recent) pick.
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];
    GuestDraftPickId lastPickId = null!;

    for (var i = 0; i < pickSlots.Length; i++)
    {
      lastPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), pickSlots[i], i + 1, owner.Id.Value).Value;
    }

    guestDraft.Complete();

    // Act
    var result = guestDraft.ApplyVeto(lastPickId, other.Id.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.DraftNotStarted);
  }

  // ── UndoVeto ─────────────────────────────────────────────────────────────

  [Fact]
  public void UndoVeto_ShouldReturnFailure_WhenThePickIsNotVetoed()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;

    // Act
    var result = guestDraft.UndoVeto(pickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickNotVetoed);
  }

  [Fact]
  public void UndoVeto_ShouldReturnFailure_WhenTheVetoHasAlreadyBeenOverridden()
  {
    // Arrange
    var (guestDraft, _, other, pickId) = CreateVetoedPickWhereOtherCanOverride();
    guestDraft.ApplyVetoOverride(pickId, other.Id.Value);

    // Act
    var result = guestDraft.UndoVeto(pickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.CannotUndoVetoThatHasBeenOverridden);
  }

  [Fact]
  public void UndoVeto_ShouldNotRefundAnyPool_WhenTheUndoItselfFails()
  {
    // Arrange -- guards the ordering fix: the refund must only happen after
    // pick.UndoVeto() has actually succeeded, never unconditionally beforehand.
    var (guestDraft, _, other, pickId) = CreateVetoedPickWhereOtherCanOverride();
    guestDraft.ApplyVetoOverride(pickId, other.Id.Value);
    var vetoesUsedBeforeFailedUndo = other.VetoesUsed;
    var fungibleTokensUsedBeforeFailedUndo = other.FungibleTokensUsed;

    // Act
    var result = guestDraft.UndoVeto(pickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    other.VetoesUsed.Should().Be(vetoesUsedBeforeFailedUndo, "a failed undo must not refund the normal pool");
    other.FungibleTokensUsed.Should().Be(fungibleTokensUsedBeforeFailedUndo, "a failed undo must not refund the fungible pool");
  }

  [Fact]
  public void UndoVeto_ShouldSucceedAndRefundTheNormalPool_WhenTheVetoHasNotBeenOverridden()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act
    var result = guestDraft.UndoVeto(pickId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    other.VetoesUsed.Should().Be(0);
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);
    pick.IsVetoed.Should().BeFalse();
    pick.CurrentVeto.Should().BeNull();
  }

  [Fact]
  public void UndoVeto_ShouldRefundTheFungiblePool_WhenTheVetoWasSpentFromIt()
  {
    // Arrange
    var (guestDraft, _, other, pickId) = CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleToken();
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    // Act
    var result = guestDraft.UndoVeto(pickId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    other.FungibleTokensUsed.Should().Be(0);
    other.VetoesUsed.Should().Be(1, "only the fungible pool was spent on this veto, the normal pool refund is untouched");
  }

  [Fact]
  public void UndoVeto_ShouldReturnFailure_WhenStatusIsNotInProgress()
  {
    // Arrange -- checked before any pick lookup, so no picks need to exist
    var guestDraft = CreateGuestDraft(GuestDraftType.Standard);
    InviteParticipant(guestDraft);
    var anyPickId = GuestDraftPickId.CreateUnique();

    // Act
    var result = guestDraft.UndoVeto(anyPickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.DraftNotStarted);
  }

  [Fact]
  public void UndoVeto_ShouldReturnFailure_WhenPickDoesNotExist()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();
    var missingPickId = GuestDraftPickId.CreateUnique();

    // Act
    var result = guestDraft.UndoVeto(missingPickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickNotFound(missingPickId.Value));
  }

  /// <summary>
  /// Two-participant MiniMega draft where "other" has already spent their one
  /// starting veto from the normal pool (on a since-re-picked slot) but was
  /// awarded a bonus fungible token, and a fresh, most-recent, un-vetoed pick is
  /// waiting to be vetoed from that fungible pool.
  /// </summary>
  private static (GuestDraft GuestDraft, GuestDraftParticipant Owner, GuestDraftParticipant Other, GuestDraftPickId MostRecentPickId)
    CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleToken()
  {
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

    var firstPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 1, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(firstPickId, other.Id.Value);
    var secondPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 1, 2, owner.Id.Value).Value;

    return (guestDraft, owner, other, secondPickId);
  }

  /// <summary>
  /// Two-participant MiniMega draft where "other" has been awarded a bonus veto
  /// override and has already vetoed the picker's most recent pick.
  /// </summary>
  private static (GuestDraft GuestDraft, GuestDraftParticipant Picker, GuestDraftParticipant Other, GuestDraftPickId PickId)
    CreateVetoedPickWhereOtherCanOverride()
  {
    var guestDraft = CreateGuestDraft(GuestDraftType.MiniMega);
    var picker = guestDraft.Participants.Single();
    var other = InviteParticipant(guestDraft);

    List<(string Name, IReadOnlyList<int> Picks, bool HasBonusVeto, bool HasBonusVetoOverride, bool HasBonusFungibleToken)> positions =
    [
      ("A", [1], false, false, false),
      ("B", [2], false, true, false),
    ];
    guestDraft.SetCustomPositions(positions, GeneratePositionPublicId);

    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "A"), picker.Id.Value);
    guestDraft.AssignParticipantToPosition(boardPositions.Single(p => p.Name == "B"), other.Id.Value);
    guestDraft.Start();

    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 1, 1, picker.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);

    return (guestDraft, picker, other, pickId);
  }
}
