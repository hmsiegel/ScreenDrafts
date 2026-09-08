using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.UpdateGuestDraft;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

/// <summary>
/// Proves the two things flagged as unverified for ChangeType/ClearBoard:
///   1. Setting GameBoard = null on a tracked aggregate actually deletes the old
///      GameBoard row (EF's required-relationship orphan-delete) and the old
///      Position rows (either via that same mechanism or Postgres's own
///      ON DELETE CASCADE) -- not just detaches them from the in-memory graph.
///   2. ClearBoard's `foreach (var position in GameBoard.Positions)` actually
///      visits real, tracked rows and revokes bonus awards -- which only works
///      because GuestDraftRepository.GetByPublicIdForGameplayAsync (the load
///      path UpdateGuestDraftCommandHandler uses) eager-loads
///      GameBoard.Positions before ChangeType runs.
/// </summary>
public sealed class ChangeGuestDraftTypeTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ChangeType_DeletesOldBoardAndPositions_AndRevokesBonusAwardsAsync()
  {
    var ct = TestContext.Current.CancellationToken;

    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();

    // "Bonus" carries all three award flags so the revocation loop has to fire
    // for every kind, not just one -- a partial fix would still pass a
    // single-flag test.
    List<CreateGuestDraftPositionInput> initialPositions =
    [
      new()
      {
        Name = "Bonus",
        Picks = [1],
        HasBonusVeto = true,
        HasBonusVetoOverride = true,
        HasBonusFungibleToken = true,
      },
      new() { Name = "Plain", Picks = [2] },
    ];

    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 2,
      positions: initialPositions
    );

    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue("test setup must be able to add the owner as a participant");
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue("test setup must be able to add the second participant");

    var draftBeforeChange = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var oldGameBoardId = draftBeforeChange.GameBoard!.Id;
    var oldPositionIds = draftBeforeChange.GameBoard.Positions.Select(p => p.Id).ToList();
    var bonusPosition = draftBeforeChange.GameBoard.Positions.Single(p => p.Name == "Bonus");
    var plainPosition = draftBeforeChange.GameBoard.Positions.Single(p => p.Name == "Plain");

    oldPositionIds.Should().HaveCount(2, "both positions must exist before ChangeType runs");

    // Owner takes the bonus-carrying position -- AssignParticipantToPosition
    // grants the awards, giving ClearBoard something real to revoke.
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        bonusPosition.PublicId,
        owner.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue("test setup must be able to assign the owner to the bonus position");
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        plainPosition.PublicId,
        other.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue("test setup must be able to assign the other participant to the plain position");

    var gameplayBefore = await GetGameplayAsync(guestDraftPublicId, owner.UserPublicId);
    gameplayBefore.IsSuccess.Should().BeTrue();

    var ownerBefore = gameplayBefore.Value.Participants.Single(p =>
      p.ParticipantPublicId == owner.GuestDrafterPublicId
    );
    var otherBefore = gameplayBefore.Value.Participants.Single(p =>
      p.ParticipantPublicId == other.GuestDrafterPublicId
    );

    ownerBefore
      .VetoTokensRemaining.Should()
      .BeGreaterThan(
        otherBefore.VetoTokensRemaining,
        "the bonus position must have granted the owner an extra veto token"
      );
    ownerBefore
      .OverrideTokensRemaining.Should()
      .BeGreaterThan(
        otherBefore.OverrideTokensRemaining,
        "the bonus position must have granted the owner an extra veto-override token"
      );
    ownerBefore
      .FungibleTokensRemaining.Should()
      .BeGreaterThan(
        otherBefore.FungibleTokensRemaining,
        "the bonus position must have granted the owner an extra fungible token"
      );

    // ── Act: switch to a different custom-layout type while still Created ──────
    var updateResult = await Sender.Send(
      new UpdateGuestDraftCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = owner.UserPublicId,
        Type = DraftType.Super.Name,
        NumberOfPicks = 2,
        Positions =
        [
          new UpdateGuestDraftPositionInput { Name = "X", Picks = [1] },
          new UpdateGuestDraftPositionInput { Name = "Y", Picks = [2] },
        ],
      },
      ct
    );

    updateResult
      .IsSuccess.Should()
      .BeTrue("ChangeType must succeed while the draft is still Created");

    // ── Assert: old board + positions are gone at the DB level, not just
    // detached from the in-memory graph. ────────────────────────────────────
    var oldBoardStillExists = await DbContext.GuestDraftGameBoards.AnyAsync(
      gb => gb.Id == oldGameBoardId,
      ct
    );
    oldBoardStillExists
      .Should()
      .BeFalse(
        "EF's required-relationship orphan-delete must remove the old GameBoard row when GameBoard is set to null"
      );

    var oldPositionsStillExist = await DbContext
      .GuestDraftPositions.Where(p => oldPositionIds.Contains(p.Id))
      .AnyAsync(ct);
    oldPositionsStillExist
      .Should()
      .BeFalse(
        "the old Position rows must be gone too, whether via EF's own orphan-delete or Postgres's ON DELETE CASCADE"
      );

    // ── Assert: a fresh board for the new type was actually built ──────────────
    var draftAfterChange = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    draftAfterChange.GuestDraftType.Should().Be(DraftType.Super);
    draftAfterChange.GameBoard.Should().NotBeNull();
    draftAfterChange
      .GameBoard.Id.Should()
      .NotBe(oldGameBoardId, "ChangeType must build a new GameBoard, not reuse the old one");
    draftAfterChange.GameBoard.Positions.Select(p => p.Name).Should().BeEquivalentTo("X", "Y");

    // ── Assert: bonus awards were actually revoked, not silently skipped. Only
    // possible if ClearBoard's foreach visited real tracked Position rows, which
    // depends on GetByPublicIdForGameplayAsync eager-loading GameBoard.Positions.
    var gameplayAfter = await GetGameplayAsync(guestDraftPublicId, owner.UserPublicId);
    gameplayAfter.IsSuccess.Should().BeTrue();

    var ownerAfter = gameplayAfter.Value.Participants.Single(p =>
      p.ParticipantPublicId == owner.GuestDrafterPublicId
    );
    var otherAfter = gameplayAfter.Value.Participants.Single(p =>
      p.ParticipantPublicId == other.GuestDrafterPublicId
    );

    ownerAfter
      .VetoTokensRemaining.Should()
      .Be(
        otherAfter.VetoTokensRemaining,
        "the bonus veto award must have been revoked when the old board was cleared"
      );
    ownerAfter
      .OverrideTokensRemaining.Should()
      .Be(
        otherAfter.OverrideTokensRemaining,
        "the bonus veto-override award must have been revoked when the old board was cleared"
      );
    ownerAfter
      .FungibleTokensRemaining.Should()
      .Be(
        otherAfter.FungibleTokensRemaining,
        "the bonus fungible-token award must have been revoked when the old board was cleared"
      );
  }
}
