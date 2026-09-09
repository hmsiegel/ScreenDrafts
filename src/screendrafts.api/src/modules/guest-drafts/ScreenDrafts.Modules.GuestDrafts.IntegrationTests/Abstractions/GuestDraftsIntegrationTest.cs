namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// A user created for a test, together with the GuestDrafter record that must now
/// exist before the user can be added as a participant to any guest draft.
/// UserPublicId is what CallerUserPublicId/OwnerUserPublicId still expect;
/// GuestDrafterPublicId is what AddParticipant/AssignParticipantToPosition expect
/// instead of a user or (now-deleted) participant public id; GuestDrafterId is the
/// GuestDrafter's own internal id, needed only to match a loaded
/// GuestDraftParticipant back to this user via ParticipantIdValue (which stores
/// GuestDrafter ids now, not UserIds).
/// </summary>
public sealed record TestUser(
  string UserPublicId,
  string GuestDrafterPublicId,
  Guid GuestDrafterId
);

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
        guest_drafts.guest_drafts,
        guest_drafts.guest_drafters,
        guest_drafts.movies
      RESTART IDENTITY CASCADE;
      """
    );

    FakeUsersApi.Reset();
  }

  /// <summary>
  /// Registers a fake user AND its GuestDrafter record -- GuestDrafter rows are
  /// normally created by consuming UserRegisteredIntegrationEvent in production,
  /// but publishing a real event through the outbox/inbox here would be slow and
  /// flaky, so this sends CreateGuestDrafterCommand directly instead. Every
  /// GuestDrafts command still expects UserPublicId (OwnerUserPublicId,
  /// CallerUserPublicId, ...); AddParticipant/AssignParticipantToPosition expect
  /// GuestDrafterPublicId instead.
  /// </summary>
  protected async Task<TestUser> CreateUserAsync()
  {
    var ct = TestContext.Current.CancellationToken;
    var userId = Guid.NewGuid();
    var userPublicId = FakeUsersApi.RegisterUser(userId, $"u_{Faker.Random.AlphaNumeric(15)}");

    var createResult = await Sender.Send(
      new CreateDrafterCommand
      {
        UserId = userId,
        FirstName = "Test",
        LastName = "User",
      },
      ct
    );

    createResult
      .IsSuccess.Should()
      .BeTrue("test setup must be able to create a guest drafter for the new user");

    var guestDrafterPublicId = createResult.Value;
    var guestDrafterId = (
      await DbContext.Drafters.FirstAsync(d => d.PublicId == guestDrafterPublicId, ct)
    )
      .Id
      .Value;

    return new TestUser(userPublicId, guestDrafterPublicId, guestDrafterId);
  }

  /// <summary>
  /// Seeds a GuestDraftMovie directly into the local movie cache and returns its
  /// PublicId -- the value PlayPickCommand.MoviePublicId expects. Mirrors
  /// canonical Drafts' CreateMovieInDbAsync: PlayPickCommandHandler now resolves
  /// movies from guest_drafts.movies via IGuestDraftMovieRepository instead of a
  /// cross-module title lookup, so a pick's movie must already be cached here.
  /// </summary>
  protected async Task<string> CreateMovieAsync(
    string? title = null,
    int? tmdbId = null,
    string? imdbId = null,
    string? year = null
  )
  {
    var movie = Movie
      .Create(
        movieTitle: title ?? Faker.Company.CompanyName(),
        publicId: $"m_{Faker.Random.AlphaNumeric(15)}",
        mediaType: MediaType.Movie,
        id: Guid.NewGuid(),
        imdbId: imdbId,
        tmdbId: tmdbId,
        year: year
      )
      .Value;

    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    return movie.PublicId;
  }

  internal async Task<string> CreateGuestDraftAsync(
    string ownerUserPublicId,
    DraftType? type = null,
    string? title = null,
    DateOnly? draftDate = null,
    int numberOfPicks = 1,
    IReadOnlyList<GuestDraftPositionInput>? positions = null
  )
  {
    var result = await Sender.Send(
      new CreateDraftCommand
      {
        OwnerUserPublicId = ownerUserPublicId,
        Title = title ?? Faker.Company.CompanyName(),
        Type = (type ?? DraftType.Standard).Name,
        DraftDate = draftDate,
        NumberOfPicks = numberOfPicks,
        Positions = positions ?? [],
      },
      TestContext.Current.CancellationToken
    );

    result.IsSuccess.Should().BeTrue("test setup must be able to create a guest draft");
    return result.Value;
  }

  protected async Task<Result> AddParticipantAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    string guestDrafterPublicId
  )
  {
    return await Sender.Send(
      new AddParticipantCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = callerUserPublicId,
        GuestDrafterPublicId = guestDrafterPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  protected async Task<Result> AssignParticipantAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    string positionPublicId,
    string guestDrafterPublicId
  )
  {
    return await Sender.Send(
      new AssignParticipantToPositionCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        PositionPublicId = positionPublicId,
        CallerUserPublicId = callerUserPublicId,
        GuestDrafterPublicId = guestDrafterPublicId,
      },
      TestContext.Current.CancellationToken
    );
  }

  /// <summary>
  /// Resolves the internal GuestDrafter id behind a UserPublicId -- needed to
  /// match a loaded GuestDraftParticipant back to a specific user via
  /// ParticipantIdValue, when all a test has on hand is the UserPublicId string
  /// returned by CreateInProgressStandardGuestDraftAsync/
  /// CreateInProgressCustomGuestDraftAsync rather than a TestUser.
  /// </summary>
  protected async Task<Guid> GetGuestDrafterIdAsync(string userPublicId)
  {
    var ct = TestContext.Current.CancellationToken;
    var userId = (await FakeUsersApi.GetUserByPublicId(userPublicId, ct))!.UserId;
    return (await DbContext.Drafters.FirstAsync(d => d.UserId == userId, ct)).Id.Value;
  }

  /// <summary>
  /// Resolves the Drafters's own PublicId behind a UserPublicId -- the value
  /// GetGuestDraftGameplay's CallerContext.ParticipantPublicId now resolves to,
  /// when all a test has on hand is the UserPublicId string returned by
  /// CreateInProgressStandardGuestDraftAsync/CreateInProgressCustomGuestDraftAsync
  /// rather than a TestUser.
  /// </summary>
  protected async Task<string> GetGuestDrafterPublicIdAsync(string userPublicId)
  {
    var ct = TestContext.Current.CancellationToken;
    var userId = (await FakeUsersApi.GetUserByPublicId(userPublicId, ct))!.UserId;
    return (await DbContext.Drafters.FirstAsync(d => d.UserId == userId, ct)).PublicId;
  }

  protected async Task<Draft> GetGuestDraftWithBoardAsync(string guestDraftPublicId)
  {
    return await DbContext
      .Drafts.Include(gd => gd.Participants)
      .Include(gd => gd.GameBoard!)
        .ThenInclude(gb => gb.Positions)
      .FirstAsync(gd => gd.PublicId == guestDraftPublicId, TestContext.Current.CancellationToken);
  }

  /// <summary>
  /// Two-participant Standard guest draft with the fixed layout applied, both
  /// positions assigned (owner -> "A" [7,6,4,2], other -> "B" [5,3,1]), and started.
  /// Create no longer auto-adds the owner as a participant, so both the owner and
  /// the other user are added explicitly, exactly the same way.
  /// </summary>
  protected async Task<(
    string GuestDraftPublicId,
    string OwnerUserPublicId,
    string OtherUserPublicId
  )> CreateInProgressStandardGuestDraftAsync()
  {
    var ct = TestContext.Current.CancellationToken;
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();

    // Standard is a fixed draft type -- Create applies its template automatically,
    // no separate board-setup step needed.
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue("test setup must be able to add the owner as a participant");
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue("test setup must be able to add the second participant");

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");
    var positionB = guestDraft.GameBoard.Positions.Single(p => p.Name == "B");

    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        positionA.PublicId,
        owner.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue("test setup must be able to assign the owner to position A");
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        positionB.PublicId,
        other.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue("test setup must be able to assign the other participant to position B");

    var startResult = await Sender.Send(
      new SetDraftStatusCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = owner.UserPublicId,
        Action = DraftStatusAction.Start,
      },
      ct
    );
    startResult.IsSuccess.Should().BeTrue("test setup must be able to start the guest draft");

    return (guestDraftPublicId, owner.UserPublicId, other.UserPublicId);
  }

  /// <summary>
  /// Custom-layout guest draft (MiniMega by default) with one position per
  /// participant, each carrying a single unique pick slot, fully assigned and
  /// started. Used for scenarios that need more than two participants. The first
  /// user in the returned list is always the owner; every participant, owner
  /// included, is added explicitly since Create no longer auto-adds the owner.
  /// </summary>
  protected async Task<(
    string GuestDraftPublicId,
    IReadOnlyList<TestUser> Users
  )> CreateInProgressCustomGuestDraftAsync(int participantCount, DraftType? type = null)
  {
    var ct = TestContext.Current.CancellationToken;
    var users = new List<TestUser>();

    for (var i = 0; i < participantCount; i++)
    {
      users.Add(await CreateUserAsync());
    }

    var owner = users[0];

    var positions = Enumerable
      .Range(0, participantCount)
      .Select(i => new GuestDraftPositionInput { Name = $"Position {i + 1}", Picks = [i + 1] })
      .ToList();

    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      type ?? DraftType.MiniMega,
      numberOfPicks: participantCount,
      positions: positions
    );

    foreach (var user in users)
    {
      (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, user.GuestDrafterPublicId))
        .IsSuccess.Should()
        .BeTrue("test setup must be able to add every participant, including the owner");
    }

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();

    for (var i = 0; i < users.Count; i++)
    {
      (
        await AssignParticipantAsync(
          guestDraftPublicId,
          owner.UserPublicId,
          boardPositions[i].PublicId,
          users[i].GuestDrafterPublicId
        )
      )
        .IsSuccess.Should()
        .BeTrue("test setup must be able to assign every participant to a position");
    }

    var startResult = await Sender.Send(
      new SetDraftStatusCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = owner.UserPublicId,
        Action = DraftStatusAction.Start,
      },
      ct
    );
    startResult.IsSuccess.Should().BeTrue("test setup must be able to start the guest draft");

    return (guestDraftPublicId, users);
  }

  // ── Thin per-command wrappers ────────────────────────────────────────────

  internal async Task<Result> UpdateGuestDraftAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    string? title = null,
    DateOnly? draftDate = null,
    DraftType? type = null,
    int? numberOfPicks = null,
    IReadOnlyList<GuestDraftPositionInput>? positions = null
  )
  {
    return await Sender.Send(
      new UpdateDraftCommand
      {
        GuestDraftPublicId = guestDraftPublicId,
        CallerUserPublicId = callerUserPublicId,
        Title = title,
        DraftDate = draftDate,
        Type = type?.Name,
        NumberOfPicks = numberOfPicks,
        Positions = positions ?? [],
      },
      TestContext.Current.CancellationToken
    );
  }

  internal async Task<Result<SetGuestDraftStatusResponse>> SetGuestDraftStatusAsync(
    string guestDraftPublicId,
    string callerUserPublicId,
    DraftStatusAction action
  )
  {
    return await Sender.Send(
      new SetDraftStatusCommand
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
      new GetDraftGameplayQuery
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
    TestUser Owner,
    TestUser B,
    TestUser C,
    TestUser D
  )> CreateGuestDraftWithMixedPickStatesAsync()
  {
    var owner = await CreateUserAsync();
    var b = await CreateUserAsync();
    var c = await CreateUserAsync();
    var d = await CreateUserAsync();

    List<GuestDraftPositionInput> positions =
    [
      new() { Name = "Owner", Picks = [1, 2] },
      new() { Name = "B", Picks = [3] },
      new()
      {
        Name = "C",
        Picks = [4],
        HasBonusVetoOverride = true,
      },
      new()
      {
        Name = "D",
        Picks = [5],
        HasBonusVetoOverride = true,
      },
    ];

    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 5,
      positions: positions
    );
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue();
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, b.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue();
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, c.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue();
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, d.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue();

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();

    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        boardPositions.Single(p => p.Name == "Owner").PublicId,
        owner.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue();
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        boardPositions.Single(p => p.Name == "B").PublicId,
        b.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue();
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        boardPositions.Single(p => p.Name == "C").PublicId,
        c.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue();
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        boardPositions.Single(p => p.Name == "D").PublicId,
        d.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue();

    (
      await SetGuestDraftStatusAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        DraftStatusAction.Start
      )
    )
      .IsSuccess.Should()
      .BeTrue();

    // slot1 (playOrder1): plain landed, left for the caller to reveal (or not).
    (await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, await CreateMovieAsync(), 1, 1))
      .IsSuccess.Should()
      .BeTrue();

    // slot2 (playOrder2): plain landed, left unrevealed.
    (await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, await CreateMovieAsync(), 2, 2))
      .IsSuccess.Should()
      .BeTrue();

    // slot3 (playOrder3): commissioner-overridden. Must happen immediately -- the
    // scope guard requires this to still be the most-recently-played pick.
    (await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, await CreateMovieAsync(), 3, 3))
      .IsSuccess.Should()
      .BeTrue();
    (await ApplyCommissionerOverrideAsync(guestDraftPublicId, 3, owner.UserPublicId))
      .IsSuccess.Should()
      .BeTrue();

    // slot4 (playOrder4): vetoed by B, then overridden by C. ApplyVeto's scope guard
    // requires this to happen immediately (most-recently-played pick); the override
    // itself has no such guard.
    (await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, await CreateMovieAsync(), 4, 4))
      .IsSuccess.Should()
      .BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 4, b.UserPublicId)).IsSuccess.Should().BeTrue();
    (await ApplyVetoOverrideAsync(guestDraftPublicId, 4, c.UserPublicId))
      .IsSuccess.Should()
      .BeTrue();

    // slot5 (playOrder5): vetoed by D (seq1), D self-overrides (seq1 overridden),
    // then C vetoes again (seq2, still active) -- both ApplyVeto calls must happen
    // while slot5 remains the most-recently-played pick, i.e. before anything else
    // is played.
    (
      await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, await CreateMovieAsync(), 5, 5)
    )
      .IsSuccess.Should()
      .BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 5, d.UserPublicId)).IsSuccess.Should().BeTrue();
    (await ApplyVetoOverrideAsync(guestDraftPublicId, 5, d.UserPublicId))
      .IsSuccess.Should()
      .BeTrue();
    (await ApplyVetoAsync(guestDraftPublicId, 5, c.UserPublicId)).IsSuccess.Should().BeTrue();

    return (guestDraftPublicId, owner, b, c, d);
  }
}
