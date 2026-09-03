namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Scenarios;

/// <summary>
/// End-to-end happy-path coverage through the full GuestDraft lifecycle, exercised
/// entirely through the command layer (matching every other GuestDrafts integration
/// test in this project).
///
/// Neither flow includes a veto-override step: ApplyVetoOverride has no Features-layer
/// endpoint/command/handler yet (route, permission, and OpenAPI name are defined, but
/// nothing wires them up -- see the domain-level coverage in
/// ScreenDrafts.Modules.GuestDrafts.UnitTests instead). Both flows still exercise
/// ApplyVeto/UndoVeto and ApplyCommissionerOverride/UndoPick in full.
/// </summary>
public sealed class FullDraftFlowTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task FullFlow_WithAFixedStandardLayoutAndTwoParticipants_ShouldReachCompletedAsync()
  {
    var ct = TestContext.Current.CancellationToken;

    // 1. Create as owner A, invite participant B
    var a = CreateUser();
    var b = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(a, GuestDraftType.Standard, "Weekend Standard Draft");
    (await InviteParticipantAsync(guestDraftPublicId, a, b)).IsSuccess.Should().BeTrue();

    // 2. Set up the fixed board layout
    (await SetFixedBoardLayoutAsync(guestDraftPublicId, a)).IsSuccess.Should().BeTrue();

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positions = guestDraft.GameBoard!.Positions;
    positions.Single(p => p.Name == "A").Picks.Should().BeEquivalentTo([7, 6, 4, 2]);
    positions.Single(p => p.Name == "B").Picks.Should().BeEquivalentTo([5, 3, 1]);

    var aUserId = (await FakeUsersApi.GetUserByPublicId(a, ct))!.UserId;
    var bUserId = (await FakeUsersApi.GetUserByPublicId(b, ct))!.UserId;
    var aParticipant = guestDraft.Participants.Single(p => p.UserId == aUserId);
    var bParticipant = guestDraft.Participants.Single(p => p.UserId == bUserId);
    var positionA = positions.Single(p => p.Name == "A");
    var positionB = positions.Single(p => p.Name == "B");

    // 3. Assign participants to positions
    (await AssignParticipantAsync(guestDraftPublicId, a, positionA.PublicId, aParticipant.PublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, a, positionB.PublicId, bParticipant.PublicId))
      .IsSuccess.Should().BeTrue();

    // 4. Start
    var startResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a, GuestDraftStatusAction.Start);
    startResult.IsSuccess.Should().BeTrue();
    startResult.Value.Status.Should().Be(GuestDraftStatus.InProgress.Name);

    // 5. Play a pick, veto it
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 7, 1)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 1, b)).IsSuccess.Should().BeTrue();

    var afterVeto = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterVeto.Picks.Single(p => p.PlayOrder == 1).IsVetoed.Should().BeTrue();

    // 6. Play another pick, apply a commissioner override to it
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 6, 2)).IsSuccess.Should().BeTrue();
    (await ApplyCommissionerOverrideAsync(guestDraftPublicId, 2, a)).IsSuccess.Should().BeTrue();

    // 7. Undo the veto -- restores slot 7 to landed
    (await UndoVetoAsync(guestDraftPublicId, 1, a)).IsSuccess.Should().BeTrue();
    var afterUndoVeto = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterUndoVeto.Picks.Single(p => p.PlayOrder == 1).IsActiveOnFinalBoard.Should().BeTrue();

    // 8. Undo the (commissioner-overridden) pick -- frees slot 6 again
    (await UndoPickAsync(guestDraftPublicId, 2, a)).IsSuccess.Should().BeTrue();
    var afterUndoPick = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterUndoPick.Picks.Should().ContainSingle(p => p.PlayOrder == 1);

    // 9. Reveal the surviving pick -- B is the auto-assigned revealer (exactly 2 participants)
    (await RevealPickAsync(guestDraftPublicId, 1, b)).IsSuccess.Should().BeTrue();

    // 10. Land every remaining slot (6, 4, 2, 5, 3, 1) so the board can complete
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 6, 3)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 4, 4)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 2, 5)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 5, 6)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 3, 7)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 1, 8)).IsSuccess.Should().BeTrue();

    // 11. Complete
    var completeResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a, GuestDraftStatusAction.Complete);
    completeResult.IsSuccess.Should().BeTrue();
    completeResult.Value.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    completeResult.Value.Status.Should().Be(GuestDraftStatus.Completed.Name);
  }

  [Fact]
  public async Task FullFlow_WithACustomLayoutAndThreeParticipants_ShouldReachCompletedAsync()
  {
    var ct = TestContext.Current.CancellationToken;

    // 1. Create as owner A, invite B and C
    var a = CreateUser();
    var b = CreateUser();
    var c = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(a, GuestDraftType.MiniMega, "Three-Way MiniMega Draft");
    (await InviteParticipantAsync(guestDraftPublicId, a, b)).IsSuccess.Should().BeTrue();
    (await InviteParticipantAsync(guestDraftPublicId, a, c)).IsSuccess.Should().BeTrue();

    // 2. Set up a custom board layout, one position per participant
    List<PositionInput> positions =
    [
      new() { Name = "Pos1", Picks = [1] },
      new() { Name = "Pos2", Picks = [2] },
      new() { Name = "Pos3", Picks = [3] },
    ];
    (await SetCustomPositionsAsync(guestDraftPublicId, a, positions)).IsSuccess.Should().BeTrue();

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    var aUserId = (await FakeUsersApi.GetUserByPublicId(a, ct))!.UserId;
    var bUserId = (await FakeUsersApi.GetUserByPublicId(b, ct))!.UserId;
    var cUserId = (await FakeUsersApi.GetUserByPublicId(c, ct))!.UserId;
    var aParticipant = guestDraft.Participants.Single(p => p.UserId == aUserId);
    var bParticipant = guestDraft.Participants.Single(p => p.UserId == bUserId);
    var cParticipant = guestDraft.Participants.Single(p => p.UserId == cUserId);

    // 3. Assign participants to positions
    (await AssignParticipantAsync(guestDraftPublicId, a, boardPositions.Single(p => p.Name == "Pos1").PublicId, aParticipant.PublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, a, boardPositions.Single(p => p.Name == "Pos2").PublicId, bParticipant.PublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, a, boardPositions.Single(p => p.Name == "Pos3").PublicId, cParticipant.PublicId))
      .IsSuccess.Should().BeTrue();

    // 4. Start
    var startResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a, GuestDraftStatusAction.Start);
    startResult.IsSuccess.Should().BeTrue();
    startResult.Value.Status.Should().Be(GuestDraftStatus.InProgress.Name);

    // 5. Play a pick, veto it
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 1, 1)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 1, b)).IsSuccess.Should().BeTrue();

    // 6. Play another pick, apply a commissioner override to it
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 2, 2)).IsSuccess.Should().BeTrue();
    (await ApplyCommissionerOverrideAsync(guestDraftPublicId, 2, a)).IsSuccess.Should().BeTrue();

    // 7. Undo the veto -- restores slot 1 to landed
    (await UndoVetoAsync(guestDraftPublicId, 1, a)).IsSuccess.Should().BeTrue();

    // 8. Undo the (commissioner-overridden) pick -- frees slot 2 again
    (await UndoPickAsync(guestDraftPublicId, 2, a)).IsSuccess.Should().BeTrue();

    // 9. Reveal the surviving pick -- with 3 participants the revealer is chosen by a
    // random draw at play time (not deterministic), so resolve it from the pick itself.
    var pickAfterUndo = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var survivingPick = pickAfterUndo.Picks.Single(p => p.PlayOrder == 1);
    survivingPick.RevealAuthorizedParticipantId.Should().NotBeNull();
    var revealerParticipant = pickAfterUndo.Participants
      .Single(p => p.Id == survivingPick.RevealAuthorizedParticipantId);
    var revealerUserPublicId = revealerParticipant.UserId == bUserId ? b : c;

    (await RevealPickAsync(guestDraftPublicId, 1, revealerUserPublicId)).IsSuccess.Should().BeTrue();

    // 10. Land the remaining slots (2, 3) so the board can complete
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 2, 3)).IsSuccess.Should().BeTrue();
    (await PlayPickAsync(guestDraftPublicId, a, CreateMovie(), 3, 4)).IsSuccess.Should().BeTrue();

    // 11. Complete
    var completeResult = await SetGuestDraftStatusAsync(guestDraftPublicId, a, GuestDraftStatusAction.Complete);
    completeResult.IsSuccess.Should().BeTrue();
    completeResult.Value.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    completeResult.Value.Status.Should().Be(GuestDraftStatus.Completed.Name);
  }
}
