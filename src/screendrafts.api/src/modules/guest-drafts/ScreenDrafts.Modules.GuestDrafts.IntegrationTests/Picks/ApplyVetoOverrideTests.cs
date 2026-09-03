namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class ApplyVetoOverrideTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ApplyVetoOverride_ByTheCallerNotThePicker_OnANonStandardDraftType_ShouldSucceedAsync()
  {
    // Arrange
    var (guestDraftPublicId, picker, other) =
      await CreateInProgressMiniMegaDraftAsync(otherHasBonusOverride: true);
    await PlayPickAsync(guestDraftPublicId, picker, CreateMovie(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ApplyVetoOverride_ForStandardDraftType_ShouldFailRegardlessOfAnyOtherPreconditionAsync()
  {
    // Arrange -- Standard blocks overrides outright, before anything else is
    // checked: this draft isn't even started and has no picks or vetoes at all,
    // yet the Standard-type guard must still fire first.
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.Standard);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.VetoOverridesNotAllowedForThisDraftType.Code);
  }

  [Fact]
  public async Task ApplyVetoOverride_WhenThePickHasNeverBeenVetoed_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, picker, other) = await CreateInProgressMiniMegaDraftAsync();
    await PlayPickAsync(guestDraftPublicId, picker, CreateMovie(), 1, 1);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task ApplyVetoOverride_WhenTheVetoIsAlreadyOverridden_ShouldFailAsync()
  {
    // Arrange -- two participants, each with their own override budget, so the
    // second attempt reaches the "already overridden" check on its own merits
    // rather than tripping the budget check first.
    var owner = CreateUser();
    var firstOverrider = CreateUser();
    var secondOverrider = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.MiniMega);
    await InviteParticipantAsync(guestDraftPublicId, owner, firstOverrider);
    await InviteParticipantAsync(guestDraftPublicId, owner, secondOverrider);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2], HasBonusVetoOverride = true },
      new() { Name = "C", Picks = [3], HasBonusVetoOverride = true },
    ];
    await SetCustomPositionsAsync(guestDraftPublicId, owner, positions);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, TestContext.Current.CancellationToken))!.UserId;
    var firstUserId = (await FakeUsersApi.GetUserByPublicId(firstOverrider, TestContext.Current.CancellationToken))!.UserId;
    var secondUserId = (await FakeUsersApi.GetUserByPublicId(secondOverrider, TestContext.Current.CancellationToken))!.UserId;
    await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "A").PublicId, guestDraft.Participants.Single(p => p.UserId == ownerUserId).PublicId);
    await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "B").PublicId, guestDraft.Participants.Single(p => p.UserId == firstUserId).PublicId);
    await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "C").PublicId, guestDraft.Participants.Single(p => p.UserId == secondUserId).PublicId);
    await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Start);

    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, firstOverrider);
    await ApplyVetoOverrideAsync(guestDraftPublicId, 1, firstOverrider);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, secondOverrider);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.VetoOverrideAlreadyUsed.Code);
  }

  [Fact]
  public async Task ApplyVetoOverride_WhenTheCallerTriesToOverrideTheVetoOnTheirOwnPick_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, picker, other) = await CreateInProgressMiniMegaDraftAsync();
    await PlayPickAsync(guestDraftPublicId, picker, CreateMovie(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, picker);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotOverrideOwnPick.Code);
  }

  [Fact]
  public async Task ApplyVetoOverride_WhenCallerHasNoRemainingOverridesOrFungibleTokens_ShouldFailAsync()
  {
    // Arrange -- "other" is not awarded any override budget
    var (guestDraftPublicId, picker, other) = await CreateInProgressMiniMegaDraftAsync();
    await PlayPickAsync(guestDraftPublicId, picker, CreateMovie(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.NoRemainingVetoOverrides.Code);
  }

  [Fact]
  public async Task ApplyVetoOverride_ShouldSpendFromTheFungiblePool_WhenTheNormalOverridePoolIsExhaustedButAFungibleTokenExistsAsync()
  {
    // Arrange -- exhaust "other"'s one awarded override on a first pick, then have
    // the picker veto their own second pick so "other"'s fungible token (not their
    // override pool) is the only thing left to pay for the second override with.
    var (guestDraftPublicId, picker, other) = await CreateInProgressMiniMegaDraftAsync(
      otherHasBonusOverride: true,
      otherHasBonusFungibleToken: true);

    await PlayPickAsync(guestDraftPublicId, picker, CreateMovie(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);
    await ApplyVetoOverrideAsync(guestDraftPublicId, 1, other);

    await PlayPickAsync(guestDraftPublicId, picker, CreateMovie(), 2, 2);
    await ApplyVetoAsync(guestDraftPublicId, 2, picker);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 2, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);
    otherParticipant.VetoOverridesUsed.Should().Be(1, "the normal override pool was already exhausted and must stay untouched");
    otherParticipant.FungibleTokensUsed.Should().Be(1);
  }

  [Fact]
  public async Task ApplyVetoOverride_WhenTheDraftIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- still Created, never started. MiniMega (not Standard) so the
    // Type check passes through to the Status check; the handler must check
    // Status before resolving PlayOrder -> pick, otherwise this would surface
    // PickNotFoundByPlayOrder (no picks exist yet) instead of DraftNotStarted.
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.MiniMega);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DraftNotStarted.Code);
  }

  [Fact]
  public async Task ApplyVetoOverride_WhenCallerIsNotAParticipant_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, picker, other) = await CreateInProgressMiniMegaDraftAsync();
    await PlayPickAsync(guestDraftPublicId, picker, CreateMovie(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);
    var stranger = CreateUser();

    // Act
    var result = await ApplyVetoOverrideAsync(guestDraftPublicId, 1, stranger);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CallerNotAParticipant.Code);
  }

  /// <summary>
  /// Two-participant MiniMega draft: picker holds position "A" ([1]), "other"
  /// holds position "B" ([2]), optionally with bonus veto-override / fungible-token
  /// awards, started and ready for picks.
  /// </summary>
  private async Task<(string GuestDraftPublicId, string Picker, string Other)> CreateInProgressMiniMegaDraftAsync(
    bool otherHasBonusOverride = false,
    bool otherHasBonusFungibleToken = false)
  {
    var picker = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(picker, GuestDraftType.MiniMega);
    await InviteParticipantAsync(guestDraftPublicId, picker, other);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new()
      {
        Name = "B",
        Picks = [2],
        HasBonusVetoOverride = otherHasBonusOverride,
        HasBonusFungibleToken = otherHasBonusFungibleToken,
      },
    ];
    await SetCustomPositionsAsync(guestDraftPublicId, picker, positions);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    var pickerUserId = (await FakeUsersApi.GetUserByPublicId(picker, TestContext.Current.CancellationToken))!.UserId;
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var pickerParticipant = guestDraft.Participants.Single(p => p.UserId == pickerUserId);
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);

    await AssignParticipantAsync(guestDraftPublicId, picker, boardPositions.Single(p => p.Name == "A").PublicId, pickerParticipant.PublicId);
    await AssignParticipantAsync(guestDraftPublicId, picker, boardPositions.Single(p => p.Name == "B").PublicId, otherParticipant.PublicId);
    await SetGuestDraftStatusAsync(guestDraftPublicId, picker, GuestDraftStatusAction.Start);

    return (guestDraftPublicId, picker, other);
  }
}
