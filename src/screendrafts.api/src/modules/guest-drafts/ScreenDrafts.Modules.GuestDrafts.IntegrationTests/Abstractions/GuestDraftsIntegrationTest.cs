namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1051:Do not declare visible instance fields",
  Justification = "Reviewed"
)]
public abstract class GuestDraftsIntegrationTest(GuestDraftsIntegrationTestWebAppFactory factory)
  : BaseIntegrationTest<GuestDraftsDbContext>(factory)
{
  protected FakeUsersApi FakeUsersApi { get; } =
    factory.Services.GetRequiredService<FakeUsersApi>();
  protected FakeMovieTitleReader FakeMovieTitleReader { get; } =
    factory.Services.GetRequiredService<FakeMovieTitleReader>();

  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE
        guest_drafts.outbox_messages,
        guest_drafts.outbox_message_consumers,
        guest_drafts.inbox_messages,
        guest_drafts.inbox_message_consumers,
        guest_drafts.guest_draft_commissioner_overrides,
        guest_drafts.guest_draft_veto_overrides,
        guest_drafts.guest_draft_vetoes,
        guest_drafts.guest_draft_pick_history,
        guest_drafts.guest_draft_picks,
        guest_drafts.guest_draft_positions,
        guest_drafts.guest_draft_game_boards,
        guest_drafts.guest_draft_participants,
        guest_drafts.guest_drafts
      RESTART IDENTITY CASCADE;
      """
    );

    FakeUsersApi.Reset();
    FakeMovieTitleReader.Reset();
  }

  /// <summary>
  /// Registers a fake user and returns its UserPublicId -- the value every
  /// GuestDrafts command expects (OwnerUserPublicId, CallerUserPublicId,
  /// InviteeUserPublicId, ...).
  /// </summary>
  protected string CreateUser() =>
    FakeUsersApi.RegisterUser(Guid.NewGuid(), $"u_{Faker.Random.AlphaNumeric(15)}");

  /// <summary>
  /// Registers a fake movie and returns its MoviePublicId -- the value
  /// PlayPickCommand.MoviePublicId expects.
  /// </summary>
  protected string CreateMovie(string? title = null) =>
    FakeMovieTitleReader.RegisterMovie(
      $"m_{Faker.Random.AlphaNumeric(15)}",
      title ?? Faker.Company.CompanyName()
    );

  protected async Task<string> CreateGuestDraftAsync(
    string ownerUserPublicId,
    GuestDraftType? type = null,
    string? title = null
  )
  {
    var result = await Sender.Send(
      new CreateGuestDraftCommand
      {
        OwnerUserPublicId = ownerUserPublicId,
        Title = title ?? Faker.Company.CompanyName(),
        Type = (type ?? GuestDraftType.Standard).Name,
      },
      TestContext.Current.CancellationToken
    );

    result.IsSuccess.Should().BeTrue("test setup must be able to create a guest draft");
    return result.Value;
  }

  protected async Task<Result> InviteParticipantAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    string inviteeUserPublicId
  )
  {
    return await Sender.Send(
      new InviteParticipantCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = callerUserPublicId,
        InviteeUserPublicId = inviteeUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> AssignParticipantAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    string positionPublicId,
    string participantPublicId
  )
  {
    return await Sender.Send(
      new AssignParticipantToPositionCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PositionPublicId = positionPublicId,
        CallerUserPublicId = callerUserPublicId,
        ParticipantPublicId = participantPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<GuestDraft> GetGuestDraftWithBoardAsync(string guestDraftPublicId)
  {
    return await DbContext
      .GuestDrafts.Include(gd => gd.Participants)
      .Include(gd => gd.GameBoard!)
        .ThenInclude(gb => gb.Positions)
      .FirstAsync(gd => gd.PublicId == guestDraftPublicId, TestContext.Current.CancellationToken);
  }

  /// <summary>
  /// Two-participant Standard guest draft with the fixed layout applied, both
  /// positions assigned (owner -> "A" [7,6,4,2], other -> "B" [5,3,1]), and started.
  /// </summary>
  protected async Task<(
    string GuestDraftPublicId,
    string OwnerUserPublicId,
    string OtherUserPublicId,
    string OwnerParticipantPublicId,
    string OtherParticipantPublicId
  )> CreateInProgressStandardGuestDraftAsync()
  {
    var ct = TestContext.Current.CancellationToken;
    var owner = CreateUser();
    var other = CreateUser();

    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.Standard);
    (await InviteParticipantAsync(guestDraftPublicId, owner, other))
      .IsSuccess.Should()
      .BeTrue("test setup must be able to invite the second participant");

    var layoutResult = await Sender.Send(
      new SetFixedBoardLayoutCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = owner,
      },
      ct
    );
    layoutResult.IsSuccess.Should().BeTrue("test setup must be able to set the fixed board layout");

    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, ct))!.UserId;
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, ct))!.UserId;

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var ownerParticipant = guestDraft.Participants.Single(p => p.UserId == ownerUserId);
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");
    var positionB = guestDraft.GameBoard.Positions.Single(p => p.Name == "B");

    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner,
        positionA.PublicId,
        ownerParticipant.PublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue("test setup must be able to assign the owner to position A");
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner,
        positionB.PublicId,
        otherParticipant.PublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue("test setup must be able to assign the other participant to position B");

    var startResult = await Sender.Send(
      new SetGuestDraftStatusCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = owner,
        Action = GuestDraftStatusAction.Start,
      },
      ct
    );
    startResult.IsSuccess.Should().BeTrue("test setup must be able to start the guest draft");

    return (guestDraftPublicId, owner, other, ownerParticipant.PublicId, otherParticipant.PublicId);
  }

  /// <summary>
  /// Custom-layout guest draft (MiniMega by default) with one position per
  /// participant, each carrying a single unique pick slot, fully assigned and
  /// started. Used for scenarios that need more than two participants. The first
  /// user in the returned list is always the owner.
  /// </summary>
  protected async Task<(
    string GuestDraftPublicId,
    IReadOnlyList<string> UserPublicIds,
    IReadOnlyList<string> ParticipantPublicIds
  )> CreateInProgressCustomGuestDraftAsync(int participantCount, GuestDraftType? type = null)
  {
    var ct = TestContext.Current.CancellationToken;
    var users = new List<string> { CreateUser() };

    for (var i = 1; i < participantCount; i++)
    {
      users.Add(CreateUser());
    }

    var owner = users[0];
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, type ?? GuestDraftType.MiniMega);

    for (var i = 1; i < users.Count; i++)
    {
      (await InviteParticipantAsync(guestDraftPublicId, owner, users[i]))
        .IsSuccess.Should()
        .BeTrue("test setup must be able to invite every participant");
    }

    var positions = Enumerable
      .Range(0, participantCount)
      .Select(i => new PositionInput { Name = $"Position {i + 1}", Picks = [i + 1] })
      .ToList();

    var customResult = await Sender.Send(
      new SetCustomPositionsCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = owner,
        Positions = positions,
      },
      ct
    );
    customResult.IsSuccess.Should().BeTrue("test setup must be able to set custom positions");

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    var participantPublicIds = new List<string>();

    for (var i = 0; i < users.Count; i++)
    {
      var userId = (await FakeUsersApi.GetUserByPublicId(users[i], ct))!.UserId;
      var participant = guestDraft.Participants.Single(p => p.UserId == userId);
      participantPublicIds.Add(participant.PublicId);

      (
        await AssignParticipantAsync(
          guestDraftPublicId,
          owner,
          boardPositions[i].PublicId,
          participant.PublicId
        )
      )
        .IsSuccess.Should()
        .BeTrue("test setup must be able to assign every participant to a position");
    }

    var startResult = await Sender.Send(
      new SetGuestDraftStatusCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = owner,
        Action = GuestDraftStatusAction.Start,
      },
      ct
    );
    startResult.IsSuccess.Should().BeTrue("test setup must be able to start the guest draft");

    return (guestDraftPublicId, users, participantPublicIds);
  }

  // ── Thin per-command wrappers ────────────────────────────────────────────

  protected async Task<Result> SetFixedBoardLayoutAsync(
    string guestDraftPublicId,
    string callerUserPublicId
  )
  {
    return await Sender.Send(
      new SetFixedBoardLayoutCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = callerUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  internal async Task<Result> SetCustomPositionsAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    IReadOnlyList<PositionInput> positions
  )
  {
    return await Sender.Send(
      new SetCustomPositionsCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = callerUserPublicId,
        Positions = positions,
      },
      TestContext.Current.CancellationToken
    );
  }

  internal async Task<Result<SetGuestDraftStatusResponse>> SetGuestDraftStatusAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    GuestDraftStatusAction action
  )
  {
    return await Sender.Send(
      new SetGuestDraftStatusCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = callerUserPublicId,
        Action = action,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> PlayPickAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    string moviePublicId,
    int position,
    int playOrder
  )
  {
    return await Sender.Send(
      new PlayPickCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        MoviePublicId = moviePublicId,
        Position = position,
        PlayOrder = playOrder,
        CallerUserPublicId = callerUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> ApplyVetoAsync(
    string guestDraftPublicId,
    int playOrder,
    string callerUserPublicId,
    string? note = null
  )
  {
    return await Sender.Send(
      new ApplyVetoCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PlayOrder = playOrder,
        CallerUserPublicId = callerUserPublicId,
        Note = note,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> ApplyCommissionerOverrideAsync(
    string guestDraftPublicId,
    int playOrder,
    string callerUserPublicId
  )
  {
    return await Sender.Send(
      new ApplyCommissionerOverrideCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PlayOrder = playOrder,
        CallerUserPublicId = callerUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> UndoVetoAsync(
    string guestDraftPublicId,
    int playOrder,
    string callerUserPublicId
  )
  {
    return await Sender.Send(
      new UndoVetoCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PlayOrder = playOrder,
        CallerUserPublicId = callerUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> UndoPickAsync(
    string guestDraftPublicId,
    int playOrder,
    string callerUserPublicId
  )
  {
    return await Sender.Send(
      new UndoPickCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PlayOrder = playOrder,
        CallerUserPublicId = callerUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> RevealPickAsync(
    string guestDraftPublicId,
    int playOrder,
    string callerUserPublicId
  )
  {
    return await Sender.Send(
      new RevealPickCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PlayOrder = playOrder,
        CallerUserPublicId = callerUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> ApplyVetoOverrideAsync(
    string guestDraftPublicId,
    int playOrder,
    string callerUserPublicId,
    string? note = null
  )
  {
    return await Sender.Send(
      new ApplyVetoOverrideCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PlayOrder = playOrder,
        CallerUserPublicId = callerUserPublicId,
        Note = note,
      },
      TestContext.Current.CancellationToken
    );
  }

  internal async Task<Result<GetGuestDraftGameplayResponse>> GetGameplayAsync(
    string guestDraftPublicId,
    string callerUserPublicId
  )
  {
    return await Sender.Send(
      new GetGuestDraftGameplayQuery
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = callerUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  /// <summary>
  /// Custom MiniMega guest draft (4 participants: Owner, B, C, D) with five distinct
  /// pick slots exercising every pick state GetGuestDraftGameplay needs to shape a
  /// response around:
  ///   - slot1: plain landed pick, left for the caller to reveal or not
  ///   - slot2: plain landed pick, left unrevealed
  ///   - slot3: commissioner-overridden (not landed)
  ///   - slot4: vetoed by B then overridden by C (landed via the override)
  ///   - slot5: vetoed by D, D self-overrides, then C vetoes again -- ends up
  ///     vetoed (not landed) with a 2-entry VetoHistory ordered by Sequence
  /// Owner plays every pick (mirrors FullDraftFlowTests' single-picker pattern).
  /// Positions: Owner->[1,2], B->[3], C->[4] (HasBonusVetoOverride), D->[5]
  /// (HasBonusVetoOverride). The draft is left InProgress -- callers decide whether
  /// and how to reveal slot1/slot2 themselves.
  /// </summary>
  protected async Task<(
    string GuestDraftPublicId,
    string OwnerUserPublicId,
    string BUserPublicId,
    string CUserPublicId,
    string DUserPublicId
  )> CreateGuestDraftWithMixedPickStatesAsync()
  {
    var ct = TestContext.Current.CancellationToken;
    var owner = CreateUser();
    var b = CreateUser();
    var c = CreateUser();
    var d = CreateUser();

    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.MiniMega);
    (await InviteParticipantAsync(guestDraftPublicId, owner, b)).IsSuccess.Should().BeTrue();
    (await InviteParticipantAsync(guestDraftPublicId, owner, c)).IsSuccess.Should().BeTrue();
    (await InviteParticipantAsync(guestDraftPublicId, owner, d)).IsSuccess.Should().BeTrue();

    List<PositionInput> positions =
    [
      new() { Name = "Owner", Picks = [1, 2] },
      new() { Name = "B", Picks = [3] },
      new() { Name = "C", Picks = [4], HasBonusVetoOverride = true },
      new() { Name = "D", Picks = [5], HasBonusVetoOverride = true },
    ];
    (await SetCustomPositionsAsync(guestDraftPublicId, owner, positions)).IsSuccess.Should().BeTrue();

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, ct))!.UserId;
    var bUserId = (await FakeUsersApi.GetUserByPublicId(b, ct))!.UserId;
    var cUserId = (await FakeUsersApi.GetUserByPublicId(c, ct))!.UserId;
    var dUserId = (await FakeUsersApi.GetUserByPublicId(d, ct))!.UserId;
    var ownerParticipant = guestDraft.Participants.Single(p => p.UserId == ownerUserId);
    var bParticipant = guestDraft.Participants.Single(p => p.UserId == bUserId);
    var cParticipant = guestDraft.Participants.Single(p => p.UserId == cUserId);
    var dParticipant = guestDraft.Participants.Single(p => p.UserId == dUserId);

    (await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "Owner").PublicId, ownerParticipant.PublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "B").PublicId, bParticipant.PublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "C").PublicId, cParticipant.PublicId))
      .IsSuccess.Should().BeTrue();
    (await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "D").PublicId, dParticipant.PublicId))
      .IsSuccess.Should().BeTrue();

    (await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Start))
      .IsSuccess.Should().BeTrue();

    // slot1 (playOrder1): plain landed, left for the caller to reveal (or not).
    (await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 1, 1)).IsSuccess.Should().BeTrue();

    // slot2 (playOrder2): plain landed, left unrevealed.
    (await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 2, 2)).IsSuccess.Should().BeTrue();

    // slot3 (playOrder3): commissioner-overridden. Must happen immediately -- the
    // scope guard requires this to still be the most-recently-played pick.
    (await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 3, 3)).IsSuccess.Should().BeTrue();
    (await ApplyCommissionerOverrideAsync(guestDraftPublicId, 3, owner)).IsSuccess.Should().BeTrue();

    // slot4 (playOrder4): vetoed by B, then overridden by C. ApplyVeto's scope guard
    // requires this to happen immediately (most-recently-played pick); the override
    // itself has no such guard.
    (await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 4, 4)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 4, b)).IsSuccess.Should().BeTrue();
    (await ApplyVetoOverrideAsync(guestDraftPublicId, 4, c)).IsSuccess.Should().BeTrue();

    // slot5 (playOrder5): vetoed by D (seq1), D self-overrides (seq1 overridden),
    // then C vetoes again (seq2, still active) -- both ApplyVeto calls must happen
    // while slot5 remains the most-recently-played pick, i.e. before anything else
    // is played.
    (await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 5, 5)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 5, d)).IsSuccess.Should().BeTrue();
    (await ApplyVetoOverrideAsync(guestDraftPublicId, 5, d)).IsSuccess.Should().BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 5, c)).IsSuccess.Should().BeTrue();

    return (guestDraftPublicId, owner, b, c, d);
  }
}
