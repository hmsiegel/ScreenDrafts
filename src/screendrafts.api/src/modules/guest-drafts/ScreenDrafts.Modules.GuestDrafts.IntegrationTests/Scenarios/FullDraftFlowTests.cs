namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Scenarios;

/// <summary>
/// End-to-end happy-path coverage through the full GuestDraft lifecycle, exercised
/// entirely through the command layer (matching every other GuestDrafts integration
/// test in this project).
///
/// Only the MiniMega (3-participant) flow exercises ApplyVetoOverride -- Standard
/// blocks veto overrides outright (GuestDraftErrors.VetoOverridesNotAllowedForThisDraftType,
/// covered directly in ApplyVetoOverrideTests), so the Standard (2-participant) flow
/// deliberately leaves that step out rather than exercising a call that can only fail.
/// Both flows still exercise ApplyVeto/UndoVeto and ApplyCommissionerOverride/UndoPick
/// in full.
/// </summary>
public sealed class FullDraftFlowTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task FullFlow_WithAFixedStandardLayoutAndTwoParticipants_ShouldReachCompletedAsync()
  {
    // 1. Create as owner A, add A and B as participants (Create no longer
    // auto-adds the owner)
    var a = await CreateUserAsync();
    var b = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(a.UserPublicId, GuestDraftType.Standard, "Weekend Standard Draft");
    (await AddParticipantAsync(guestDraftPublicId, a.UserPublicId, a.GuestDrafterPublicId)).IsSuccess.Should().BeTrue();
    (await AddParticipantAsync(guestDraftPublicId, a.UserPublicId, b.GuestDrafterPublicId)).IsSuccess.Should().BeTrue();

    // 2. Standard is a fixed type -- Create already applied its template automatically
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positions = guestDraft.GameBoard!.Positions;
    positions.Single(p => p.Name == "A").Picks.Should().BeEquivalentTo([7, 6, 4, 2]);
    positions.Single(p => p.Name == "B").Picks.Should().BeEquivalentTo([5, 3, 1]);

    var positionA = positions.Single(p => p.Name == "A");
    var positionB = positions.Single(p => p.Name == "B");

    // 3. Assign participants to positions
    (await AssignParticipantAsync(guestDraftPublicId, a.UserPublicId, positionA.PublicId, a.GuestDrafterPublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, a.UserPublicId, positionB.PublicId, b.GuestDrafterPublicId))
      .IsSuccess.Should().BeTrue();

    // 4. Start
    var startResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a.UserPublicId, GuestDraftStatusAction.Start);
    startResult.IsSuccess.Should().BeTrue();
    startResult.Value.Status.Should().Be(GuestDraftStatus.InProgress.Name);

    // 5. Play a pick, veto it
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 7, 1)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 1, b.UserPublicId)).IsSuccess.Should().BeTrue();

    var afterVeto = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterVeto.Picks.Single(p => p.PlayOrder == 1).IsVetoed.Should().BeTrue();

    // 6. Play another pick, apply a commissioner override to it
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 6, 2)).IsSuccess.Should().BeTrue();
    (await ApplyCommissionerOverrideAsync(guestDraftPublicId, 2, a.UserPublicId)).IsSuccess.Should().BeTrue();

    // 7. Undo the veto -- restores slot 7 to landed
    (await UndoVetoAsync(guestDraftPublicId, 1, a.UserPublicId)).IsSuccess.Should().BeTrue();
    var afterUndoVeto = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterUndoVeto.Picks.Single(p => p.PlayOrder == 1).IsActiveOnFinalBoard.Should().BeTrue();

    // 8. Undo the (commissioner-overridden) pick -- frees slot 6 again
    (await UndoPickAsync(guestDraftPublicId, 2, a.UserPublicId)).IsSuccess.Should().BeTrue();
    var afterUndoPick = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterUndoPick.Picks.Should().ContainSingle(p => p.PlayOrder == 1);

    // 9. Reveal the surviving pick -- B is the auto-assigned revealer (exactly 2 participants)
    (await RevealPickAsync(guestDraftPublicId, 1, b.UserPublicId)).IsSuccess.Should().BeTrue();

    // 10. Land every remaining slot (6, 4, 2, 5, 3, 1) so the board can complete
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 6, 3)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 4, 4)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 2, 5)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 5, 6)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 3, 7)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 1, 8)).IsSuccess.Should().BeTrue();

    // 11. Complete
    var completeResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a.UserPublicId, GuestDraftStatusAction.Complete);
    completeResult.IsSuccess.Should().BeTrue();
    completeResult.Value.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    completeResult.Value.Status.Should().Be(GuestDraftStatus.Completed.Name);
  }

  [Fact]
  public async Task FullFlow_WithACustomLayoutAndThreeParticipants_ShouldReachCompletedAsync()
  {
    // 1. Create as owner A, add A, B and C as participants
    var a = await CreateUserAsync();
    var b = await CreateUserAsync();
    var c = await CreateUserAsync();

    // 2. Custom board layout, one position per participant, supplied at Create
    // time (MiniMega is a non-fixed type). Pos2 (B) carries a bonus
    // veto-override -- MiniMega, unlike Standard, allows ApplyVetoOverride, and
    // B uses this bonus in step 11 below.
    List<CreateGuestDraftPositionInput> positions =
    [
      new() { Name = "Pos1", Picks = [1] },
      new() { Name = "Pos2", Picks = [2], HasBonusVetoOverride = true },
      new() { Name = "Pos3", Picks = [3] },
    ];
    var guestDraftPublicId = await CreateGuestDraftAsync(
      a.UserPublicId,
      GuestDraftType.MiniMega,
      "Three-Way MiniMega Draft",
      numberOfPicks: 3,
      positions: positions
    );
    (await AddParticipantAsync(guestDraftPublicId, a.UserPublicId, a.GuestDrafterPublicId)).IsSuccess.Should().BeTrue();
    (await AddParticipantAsync(guestDraftPublicId, a.UserPublicId, b.GuestDrafterPublicId)).IsSuccess.Should().BeTrue();
    (await AddParticipantAsync(guestDraftPublicId, a.UserPublicId, c.GuestDrafterPublicId)).IsSuccess.Should().BeTrue();

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();

    // 3. Assign participants to positions
    (await AssignParticipantAsync(guestDraftPublicId, a.UserPublicId, boardPositions.Single(p => p.Name == "Pos1").PublicId, a.GuestDrafterPublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, a.UserPublicId, boardPositions.Single(p => p.Name == "Pos2").PublicId, b.GuestDrafterPublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, a.UserPublicId, boardPositions.Single(p => p.Name == "Pos3").PublicId, c.GuestDrafterPublicId))
      .IsSuccess.Should().BeTrue();

    // 4. Start
    var startResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a.UserPublicId, GuestDraftStatusAction.Start);
    startResult.IsSuccess.Should().BeTrue();
    startResult.Value.Status.Should().Be(GuestDraftStatus.InProgress.Name);

    // 5. Play a pick, veto it
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 1, 1)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 1, b.UserPublicId)).IsSuccess.Should().BeTrue();

    // 6. Play another pick, apply a commissioner override to it
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 2, 2)).IsSuccess.Should().BeTrue();
    (await ApplyCommissionerOverrideAsync(guestDraftPublicId, 2, a.UserPublicId)).IsSuccess.Should().BeTrue();

    // 7. Undo the veto -- restores slot 1 to landed
    (await UndoVetoAsync(guestDraftPublicId, 1, a.UserPublicId)).IsSuccess.Should().BeTrue();

    // 8. Undo the (commissioner-overridden) pick -- frees slot 2 again
    (await UndoPickAsync(guestDraftPublicId, 2, a.UserPublicId)).IsSuccess.Should().BeTrue();

    // 9. Reveal the surviving pick -- with 3 participants the revealer is chosen by a
    // random draw at play time (not deterministic), so resolve it from the pick itself.
    var pickAfterUndo = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var survivingPick = pickAfterUndo.Picks.Single(p => p.PlayOrder == 1);
    survivingPick.RevealAuthorizedParticipantId.Should().NotBeNull();
    var revealerParticipant = pickAfterUndo.Participants
      .Single(p => p.Id == survivingPick.RevealAuthorizedParticipantId);
    var revealerUserPublicId = revealerParticipant.ParticipantIdValue == b.GuestDrafterId ? b.UserPublicId : c.UserPublicId;

    (await RevealPickAsync(guestDraftPublicId, 1, revealerUserPublicId)).IsSuccess.Should().BeTrue();

    // 10. Land slot 2
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 2, 3)).IsSuccess.Should().BeTrue();

    // 11. Land slot 3 via a veto override -- C vetoes it, then B (holding the bonus
    // override from Pos2) overrides that veto, landing the pick without a re-pick.
    // Both calls happen immediately: ApplyVeto's scope guard requires slot 3 to
    // still be the most-recently-played pick; ApplyVetoOverride has no such guard.
    (await PlayPickAsync(guestDraftPublicId, a.UserPublicId, await CreateMovieAsync(), 3, 4)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 4, c.UserPublicId)).IsSuccess.Should().BeTrue();
    (await ApplyVetoOverrideAsync(guestDraftPublicId, 4, b.UserPublicId)).IsSuccess.Should().BeTrue();

    var afterVetoOverride = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterVetoOverride.Picks.Single(p => p.PlayOrder == 4).IsActiveOnFinalBoard.Should().BeTrue();

    // 12. Complete
    var completeResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a.UserPublicId, GuestDraftStatusAction.Complete);
    completeResult.IsSuccess.Should().BeTrue();
    completeResult.Value.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    completeResult.Value.Status.Should().Be(GuestDraftStatus.Completed.Name);
  }
}
