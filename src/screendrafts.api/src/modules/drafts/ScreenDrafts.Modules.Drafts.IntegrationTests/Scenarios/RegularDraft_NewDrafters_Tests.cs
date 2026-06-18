using ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Scenarios;

/// <summary>
/// Scenario 1 — Regular Draft: Two New Drafters
///
/// Two brand-new drafters with zero prior history complete a 7-pick Standard draft.
/// Random trivia (first to 3 correct) determines positions.
/// Each drafter gets 1 regular veto.
/// Board scenario (DraftBoard, not pool).
/// </summary>
public sealed class RegularDraft_NewDrafters_Tests(DraftsIntegrationTestWebAppFactory factory)
  : DraftScenarioBase(factory)
{
  // Fixed participant data seeded during InitializeAsync
  private string _draftPublicId = default!;
  private string _draftPartPublicId = default!;
  private string _drafterAPublicId = default!;
  private string _drafterBPublicId = default!;
  private string[] _moviePublicIds = default!;

  protected override async Task OnInitializeAsync()
  {
    await Shared.SeedAsync(Sender, FakeUsersApi);
    EmailCapture.Clear();
    HubCapture.Clear();
    EventBusCapture.Clear();

    // Step 1 — CreateDraft
    _draftPublicId = await CreateDraftAsync(
      "Regular Draft — New Drafters",
      DraftType.Standard.Value,
      Shared.RegularSeriesId
    );

    // ProcessOutboxAsync dispatches the DraftCreated domain event handler which publishes
    // DraftCreatedIntegrationEvent to CapturingEventBus — available for the email test.
    await ProcessOutboxAsync();

    // Step 2 — CreateDraftPart (1 part, 7 picks → positions 1-7)
    await CreateDraftPartAsync(_draftPublicId, partIndex: 1, minPosition: 1, maxPosition: 7);
    _draftPartPublicId = await GetDraftPartPublicIdAsync(_draftPublicId);

    // Create two brand-new drafters
    var personA = await CreatePersonAsync(Faker.Name.FirstName(), Faker.Name.LastName());
    _drafterAPublicId = await CreateDrafterAsync(personA);

    var personB = await CreatePersonAsync(Faker.Name.FirstName(), Faker.Name.LastName());
    _drafterBPublicId = await CreateDrafterAsync(personB);

    // Step 3 — AddParticipants
    await AddDrafterParticipantAsync(_draftPartPublicId, _drafterAPublicId);
    await AddDrafterParticipantAsync(_draftPartPublicId, _drafterBPublicId);

    // Step 4 — AddHosts
    await AddPrimaryHostAsync(_draftPartPublicId, Shared.ClayHostPublicId);
    await AddCoHostAsync(_draftPartPublicId, Shared.RyanHostPublicId);

    // Step 5 — SetDraftPositions (trivia winner A gets odd, B gets even — will assign after trivia)
    // We use "Drafter A" and "Drafter B" as position names; actual picks will be set post-trivia
    // 2-person draft: winner gets 7 picks in positions 1,3,5,7; loser gets 2,4,6
    await SetPositionsAsync(
      _draftPartPublicId,
      [
        new DraftPositionRequest { Name = "Drafter A", Picks = [1, 3, 5, 7] },
        new DraftPositionRequest { Name = "Drafter B", Picks = [2, 4, 6] },
      ]
    );

    // Step 6 — StartDraft
    await StartDraftPartAsync(_draftPublicId);

    // Step 7 — AssignTriviaResults (Drafter A wins, reaching 3 first)
    await AssignTriviaAsync(
      _draftPartPublicId,
      [
        (_drafterAPublicId, ParticipantKind.Drafter, 1, 3),
        (_drafterBPublicId, ParticipantKind.Drafter, 2, 1),
      ]
    );

    // Step 8 — AssignParticipantsToPositions
    var posAId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Drafter A");
    var posBId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Drafter B");
    await AssignParticipantToPositionAsync(
      _draftPartPublicId,
      posAId,
      _drafterAPublicId,
      ParticipantKind.Drafter
    );
    await AssignParticipantToPositionAsync(
      _draftPartPublicId,
      posBId,
      _drafterBPublicId,
      ParticipantKind.Drafter
    );

    // Seed 7 placeholder movies
    _moviePublicIds = new string[7];
    for (var i = 0; i < 7; i++)
    {
      var movie = Movie
        .Create(
          $"Placeholder Movie {i + 1}",
          $"m_{Guid.NewGuid():N}",
          MediaType.Movie,
          Guid.NewGuid()
        )
        .Value;
      DbContext.Movies.Add(movie);
      _moviePublicIds[i] = movie.PublicId;
    }

    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Step 9: Picks loop (alternating, starting from position 7 down to 1)
  //   Drafter A: 7, 5, 3, 1
  //   Drafter B: 6, 4, 2
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task RegularDraft_NewDrafters_FullPickSequence_ShouldSucceedAsync()
  {
    // Act — execute all 7 picks in descending play order
    await PlayPickAsync(
      _draftPartPublicId,
      7,
      7,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[0]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      6,
      6,
      _drafterBPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[1]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      5,
      5,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[2]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      4,
      4,
      _drafterBPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[3]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      3,
      3,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[4]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      2,
      2,
      _drafterBPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[5]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      1,
      1,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[6]
    );

    // Assert — 7 picks total
    var picks = await DbContext
      .Picks.Where(p => p.DraftPart.PublicId == _draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    picks.Should().HaveCount(7);
  }

  [Fact]
  public async Task RegularDraft_NewDrafters_DrafterA_ShouldHaveFourPicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var drafterAId = await GetDrafterInternalIdAsync(_drafterAPublicId);
    var aCount = await DbContext.Picks.CountAsync(
      p =>
        p.DraftPart.PublicId == _draftPartPublicId
        && p.PlayedByParticipantKindValue == ParticipantKind.Drafter
        && p.PlayedByParticipantIdValue == drafterAId,
      TestContext.Current.CancellationToken
    );

    aCount.Should().Be(4, "Drafter A (trivia winner) should have 4 picks");
  }

  [Fact]
  public async Task RegularDraft_NewDrafters_DrafterB_ShouldHaveThreePicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var drafterBId = await GetDrafterInternalIdAsync(_drafterBPublicId);
    var bCount = await DbContext.Picks.CountAsync(
      p =>
        p.DraftPart.PublicId == _draftPartPublicId
        && p.PlayedByParticipantKindValue == ParticipantKind.Drafter
        && p.PlayedByParticipantIdValue == drafterBId,
      TestContext.Current.CancellationToken
    );

    bCount.Should().Be(3, "Drafter B should have 3 picks");
  }

  [Fact]
  public async Task RegularDraft_NewDrafters_EndDraft_ShouldComplete_WithLockedBoardAsync()
  {
    await PlayAllPicksAsync();

    // Step 10 — EndDraft
    await CompleteDraftPartAsync(_draftPublicId);

    var draftPart = await DbContext
      .DraftParts.AsNoTracking()
      .FirstAsync(dp => dp.PublicId == _draftPartPublicId, TestContext.Current.CancellationToken);

    draftPart.Status.Should().Be(DraftPartStatus.Completed);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Negative guard cases
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task RegularDraft_NewDrafters_AddPick_WithInvalidMovieId_ShouldFailAsync()
  {
    var result = await Sender.Send(
      new PlayPickCommand
      {
        DraftPartId = _draftPartPublicId,
        Position = 7,
        PlayOrder = 7,
        ParticipantPublicId = _drafterAPublicId,
        ParticipantKind = ParticipantKind.Drafter,
        MoviePublicId = "m_doesnotexist",
        ActedByPublicId = _drafterAPublicId,
      },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue("Picking a non-existent movie must fail");
  }

  [Fact]
  public async Task RegularDraft_NewDrafters_AddPick_WhenDraftNotStarted_ShouldFailAsync()
  {
    // Arrange: create a separate draft part that has NOT been started
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(
      "Not Started Draft",
      DraftType.Standard.Value,
      seriesId
    );
    await CreateDraftPartAsync(draftPublicId, 1, 1, 7);
    var partPublicId = await GetDraftPartPublicIdAsync(draftPublicId);
    await AddDrafterParticipantAsync(partPublicId, _drafterAPublicId);

    var movie = Movie
      .Create("Test", $"m_{Guid.NewGuid():N}", MediaType.Movie, Guid.NewGuid())
      .Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var result = await Sender.Send(
      new PlayPickCommand
      {
        DraftPartId = partPublicId,
        Position = 1,
        PlayOrder = 1,
        ParticipantPublicId = _drafterAPublicId,
        ParticipantKind = ParticipantKind.Drafter,
        MoviePublicId = movie.PublicId,
        ActedByPublicId = _drafterAPublicId,
      },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue("Picking when draft is not InProgress must fail");
  }

  [Fact]
  public async Task RegularDraft_NewDrafters_AddPick_DuplicateMovie_ShouldFailAsync()
  {
    // Play position 7 first, then try to pick the same movie at position 6
    await PlayPickAsync(
      _draftPartPublicId,
      7,
      7,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[0]
    );

    var result = await Sender.Send(
      new PlayPickCommand
      {
        DraftPartId = _draftPartPublicId,
        Position = 6,
        PlayOrder = 6,
        ParticipantPublicId = _drafterBPublicId,
        ParticipantKind = ParticipantKind.Drafter,
        MoviePublicId = _moviePublicIds[0], // same movie
        ActedByPublicId = _drafterBPublicId,
      },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue("Picking a movie already picked in this part must fail");
  }

  [Fact]
  public async Task RegularDraft_NewDrafters_ApplyVeto_WhenNoRemainingVetoes_ShouldFailAsync()
  {
    // Use Drafter A's only veto, then try again
    await PlayPickAsync(
      _draftPartPublicId,
      7,
      7,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[0]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      6,
      6,
      _drafterBPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[1]
    );
    await ApplyVetoAsync(_draftPartPublicId, 7, _drafterAPublicId, ParticipantKind.Drafter); // spends A's veto

    var result = await Sender.Send(
      new ApplyVetoCommand
      {
        DraftPartId = _draftPartPublicId,
        PlayOrder = 6,
        ParticipantPublicId = _drafterAPublicId,
        ParticipantKind = ParticipantKind.Drafter,
        ActorPublicId = _drafterAPublicId,
      },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue("Vetoing when no vetoes remain must fail");
  }

  [Fact]
  public async Task RegularDraft_NewDrafters_SetDraftPositions_WithDuplicatePositions_ShouldFailAsync()
  {
    // Arrange: a fresh draft part
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(
      "Duplicate Pos Test",
      DraftType.Standard.Value,
      seriesId
    );
    await CreateDraftPartAsync(draftPublicId, 1, 1, 4);
    var partPublicId = await GetDraftPartPublicIdAsync(draftPublicId);
    await AddDrafterParticipantAsync(partPublicId, _drafterAPublicId);
    await AddDrafterParticipantAsync(partPublicId, _drafterBPublicId);

    // Positions include duplicate pick slot (1 in both)
    var result = await Sender.Send(
      new SetDraftPositionsCommand
      {
        DraftPartId = partPublicId,
        Positions =
        [
          new DraftPositionRequest { Name = "A", Picks = [1, 3] },
          new DraftPositionRequest { Name = "B", Picks = [1, 2] }, // duplicate pick slot 1
        ],
      },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue("Duplicate pick slots in positions must fail");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Communications: email notification
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task RegularDraft_DraftCreated_ShouldSendEmailToRegisteredRecipientsAsync()
  {
    // Arrange — seed a user that the DraftCreatedIntegrationEventConsumer will find.
    // The DraftCreatedIntegrationEvent was already captured in OnInitializeAsync via
    // ProcessOutboxAsync → CapturingEventBus.
    const string recipientEmail = "drafter@screendrafts.test";
    await SeedEmailRecipientAsync(recipientEmail, "Test Drafter");

    // Act — deliver the captured integration events in-process to Communications consumers.
    await DispatchIntegrationEventsAsync();

    // Assert
    AssertDraftCreatedEmailSent("Regular Draft — New Drafters", recipientEmail);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // RealTimeUpdates: SignalR broadcasts on pick play and reveal
  //
  // PickAddedIntegrationEvent only fires once a pick is revealed (see the
  // IsRevealed guard in PickCreatedDomainEventHandler) — playing a pick alone
  // notifies only the host (PickSubmitted, to the host-only group) so the
  // movie title isn't broadcast to everyone before the host reveals it on
  // stream. These two tests reflect that split instead of asserting a single
  // unconditional "play -> PickAdded" broadcast.
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task RegularDraft_PlayPick_ShouldBroadcastPickSubmittedToHostViaSignalRAsync()
  {
    // Act — play one pick and process the outbox so the
    // PickSubmittedIntegrationEvent is captured by EventBusCapture. The pick
    // is not yet revealed, so PickAddedIntegrationEvent must NOT fire here.
    await PlayPickAsync(
      _draftPartPublicId,
      7,
      7,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[0]
    );
    await ProcessOutboxAsync();

    // Deliver the captured integration events in-process to RealTimeUpdates consumers.
    await DispatchIntegrationEventsAsync();

    // Assert — the host group received a PickSubmitted broadcast for this pick.
    HubCapture
      .SentMessages.Should()
      .Contain(
        m =>
          m.Method == "PickSubmitted" && m.GroupName == DraftHub.HostGroupName(_draftPartPublicId),
        "playing a pick should notify the host privately before it's revealed"
      );

    // Assert — no PickAdded broadcast yet, since the pick hasn't been revealed.
    HubCapture
      .SentMessages.Should()
      .NotContain(
        m => m.Method == "PickAdded",
        "PickAdded must not fire until the pick is revealed"
      );
  }

  [Fact]
  public async Task RegularDraft_RevealPick_ShouldBroadcastPickAddedViaSignalRAsync()
  {
    // Arrange — play the pick first; this alone must not trigger PickAdded.
    await PlayPickAsync(
      _draftPartPublicId,
      7,
      7,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[0]
    );
    await ProcessOutboxAsync();
    await DispatchIntegrationEventsAsync();
    HubCapture.Clear();
    EventBusCapture.Clear();

    // Act — reveal the pick. PickRevealedDomianEventHandler now publishes
    // both PickRevealedIntegrationEvent (private, host-only "reveal action
    // completed" signal) and PickAddedIntegrationEvent (public broadcast
    // that the movie is now visible on the board) from the same handler.
    var revealResult = await Sender.Send(
      new RevealPickCommand
      {
        DraftPartId = _draftPartPublicId,
        PlayOrder = 7,
        UserPublicId = Shared.ClayHostUserPublicId,
      },
      TestContext.Current.CancellationToken
    );
    revealResult.IsSuccess.Should().BeTrue();

    await ProcessOutboxAsync();
    await DispatchIntegrationEventsAsync();

    // Assert — the general draft-part group received a PickAdded broadcast,
    // which is the actual viewer-facing signal that the board changed.
    HubCapture
      .SentMessages.Should()
      .Contain(
        m => m.Method == "PickAdded" && m.GroupName == DraftHub.GroupName(_draftPartPublicId),
        "revealing a pick should broadcast PickAdded to everyone in the draft part group"
      );

    // Assert — the host also gets a private PickRevealed notification on the
    // same action; this is a separate signal, not a duplicate of PickAdded.
    HubCapture
      .SentMessages.Should()
      .Contain(
        m =>
          m.Method == "PickRevealed" && m.GroupName == DraftHub.HostGroupName(_draftPartPublicId),
        "the host group should also receive a private PickRevealed notification"
      );
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────────────────────────────────

  private async Task PlayAllPicksAsync()
  {
    await PlayPickAsync(
      _draftPartPublicId,
      7,
      7,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[0]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      6,
      6,
      _drafterBPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[1]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      5,
      5,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[2]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      4,
      4,
      _drafterBPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[3]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      3,
      3,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[4]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      2,
      2,
      _drafterBPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[5]
    );
    await PlayPickAsync(
      _draftPartPublicId,
      1,
      1,
      _drafterAPublicId,
      ParticipantKind.Drafter,
      _moviePublicIds[6]
    );
  }

  private async Task<Guid> GetDrafterInternalIdAsync(string drafterPublicId)
  {
    var id = await DbContext
      .Drafters.Where(d => d.PublicId == drafterPublicId)
      .Select(d => d.Id)
      .FirstAsync(TestContext.Current.CancellationToken);
    return id.Value;
  }
}
