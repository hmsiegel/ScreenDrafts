using Dapper;

using ScreenDrafts.Common.Application.Data;
using ScreenDrafts.Common.Application.EventBus;
using ScreenDrafts.Common.Application.Inbox;
using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;
using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;
using ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;
using ScreenDrafts.Modules.Drafts.IntegrationTests.Fixtures;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

/// <summary>
/// Abstract base class for all end-to-end draft scenario tests.
/// Provides shared setup helpers for the 11-step universal draft flow.
/// </summary>
[Collection(nameof(DraftsIntegrationTestCollection))]
public abstract class DraftScenarioBase(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  protected SharedDraftFixture Shared { get; } = new();
  protected MediaSeedFixture Media { get; } = new();

  private DraftsIntegrationTestWebAppFactory DraftsFactory =>
    (DraftsIntegrationTestWebAppFactory)Factory;

  /// <summary>Email capture — clear in OnInitializeAsync, inspect after drain.</summary>
  protected IEmailCapture EmailCapture => DraftsFactory.EmailCapture;

  /// <summary>
  /// Integration-event capture — clear in OnInitializeAsync.
  /// After ProcessOutboxAsync, call DispatchIntegrationEventsAsync to deliver to consumers.
  /// </summary>
  protected CapturingEventBus EventBusCapture => DraftsFactory.EventBusCapture;

  /// <summary>
  /// SignalR hub capture — clear in OnInitializeAsync.
  /// Populated by RealTimeUpdates consumers after DispatchIntegrationEventsAsync.
  /// </summary>
  protected DraftHubCapture HubCapture => DraftsFactory.HubCapture;

  // ───────────────────────────────────────────────────────────────────────────
  // Step 1: CreateDraft
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task<string> CreateDraftAsync(
    string title,
    int draftType,
    Guid seriesId)
  {
    var result = await Sender.Send(new CreateDraftCommand
    {
      Title = title,
      DraftType = draftType,
      SeriesId = seriesId
    });

    result.IsSuccess.Should().BeTrue($"CreateDraft '{title}' must succeed");
    return result.Value;
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 2: CreateDraftPart
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task<string> CreateDraftPartAsync(
    string draftPublicId,
    int partIndex,
    int minPosition,
    int maxPosition)
  {
    var result = await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = partIndex,
      MinimumPosition = minPosition,
      MaximumPosition = maxPosition
    });

    result.IsSuccess.Should().BeTrue($"CreateDraftPart index={partIndex} must succeed");
    return result.Value;
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 3: AddParticipant
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task AddDrafterParticipantAsync(string draftPartPublicId, string drafterPublicId)
  {
    var result = await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    result.IsSuccess.Should().BeTrue($"AddParticipant drafter={drafterPublicId} must succeed");
  }

  protected async Task AddTeamParticipantAsync(string draftPartPublicId, string teamPublicId)
  {
    var result = await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = teamPublicId,
      ParticipantKind = ParticipantKind.Team
    });

    result.IsSuccess.Should().BeTrue($"AddParticipant team={teamPublicId} must succeed");
  }

  protected async Task AddCommunityParticipantAsync(string draftPartPublicId)
  {
    var communityId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = communityId,
      ParticipantKind = ParticipantKind.Community
    });
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 4: AddHosts
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task AddPrimaryHostAsync(string draftPartPublicId, string hostPublicId)
  {
    var result = await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    result.IsSuccess.Should().BeTrue($"AddHost primary={hostPublicId} must succeed");
  }

  protected async Task AddCoHostAsync(string draftPartPublicId, string hostPublicId)
  {
    var result = await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.CoHost
    });

    result.IsSuccess.Should().BeTrue($"AddHost co-host={hostPublicId} must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 5: SetDraftPositions
  // ───────────────────────────────────────────────────────────────────────────

  internal async Task SetPositionsAsync(string draftPartPublicId, IEnumerable<DraftPositionRequest> positions)
  {
    var result = await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions = positions.ToList()
    });

    result.IsSuccess.Should().BeTrue("SetDraftPositions must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 5b: AssignParticipantsToPositions (after setting positions)
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task AssignParticipantToPositionAsync(
    string draftPartPublicId,
    string positionPublicId,
    string participantPublicId,
    ParticipantKind kind)
  {
    var result = await Sender.Send(new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId,
      ParticipantPublicId = participantPublicId,
      ParticipantKind = kind
    });

    result.IsSuccess.Should().BeTrue($"AssignParticipantToPosition {participantPublicId} must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 6: StartDraft
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task StartDraftPartAsync(string draftPublicId, int partIndex = 1)
  {
    var result = await Sender.Send(new SetDraftPartStatusCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = partIndex,
      Action = DraftPartStatusAction.Start
    });

    result.IsSuccess.Should().BeTrue($"StartDraftPart part={partIndex} must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 7: AssignTriviaResults
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task AssignTriviaAsync(
    string draftPartPublicId,
    IEnumerable<(string participantPublicId, ParticipantKind kind, int placement, int questionsWon)> results)
  {
    var entries = results.Select(r => new TriviaResultEntry
    {
      ParticipantPublicId = r.participantPublicId,
      Kind = r.kind,
      Position = r.placement,
      QuestionsWon = r.questionsWon
    });

    var cmd = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results = entries
    };

    var result = await Sender.Send(cmd);
    result.IsSuccess.Should().BeTrue("AssignTriviaResults must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 9: Picks
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task PlayPickAsync(
    string draftPartPublicId,
    int position,
    int playOrder,
    string participantPublicId,
    ParticipantKind kind,
    string moviePublicId,
    string? actedByPublicId = null)
  {
    var result = await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = position,
      PlayOrder = playOrder,
      ParticipantPublicId = participantPublicId,
      ParticipantKind = kind,
      MoviePublicId = moviePublicId,
      ActedByPublicId = actedByPublicId ?? participantPublicId
    });

    result.IsSuccess.Should().BeTrue(
      $"PlayPick playOrder={playOrder} position={position} movie={moviePublicId} must succeed");
  }

  protected async Task ApplyVetoAsync(
    string draftPartPublicId,
    int playOrder,
    string participantPublicId,
    ParticipantKind kind,
    string? actorPublicId = null)
  {
    var result = await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = playOrder,
      ParticipantPublicId = participantPublicId,
      ParticipantKind = kind,
      ActorPublicId = actorPublicId ?? participantPublicId
    });

    result.IsSuccess.Should().BeTrue(
      $"ApplyVeto playOrder={playOrder} by={participantPublicId} must succeed");
  }

  protected async Task ApplyVetoOverrideAsync(
    string draftPartPublicId,
    int playOrder,
    string participantPublicId,
    ParticipantKind kind,
    string? actorPublicId = null)
  {
    var result = await Sender.Send(new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = playOrder,
      ParticipantIdValue = participantPublicId,
      ParticipantKind = kind,
      ActorPublicId = actorPublicId ?? participantPublicId
    });

    result.IsSuccess.Should().BeTrue(
      $"ApplyVetoOverride playOrder={playOrder} by={participantPublicId} must succeed");
  }

  protected async Task ApplyCommunityVetoAsync(string draftPartPublicId, int playOrder)
  {
    var communityId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
    var result = await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = playOrder,
      ParticipantPublicId = communityId,
      ParticipantKind = ParticipantKind.Community,
      ActorPublicId = communityId
    });

    result.IsSuccess.Should().BeTrue(
      $"ApplyCommunityVeto playOrder={playOrder} must succeed");
  }

  protected async Task ApplyCommissionerOverrideAsync(string draftPartPublicId, int playOrder)
  {
    var result = await Sender.Send(new ApplyCommissionerOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = playOrder
    });

    result.IsSuccess.Should().BeTrue(
      $"ApplyCommissionerOverride playOrder={playOrder} must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Step 10: EndDraft
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task CompleteDraftPartAsync(string draftPublicId, int partIndex = 1)
  {
    var result = await Sender.Send(new SetDraftPartStatusCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = partIndex,
      Action = DraftPartStatusAction.Complete
    });

    result.IsSuccess.Should().BeTrue($"CompleteDraftPart part={partIndex} must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Pool helpers (Scenarios 3 & 4)
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task CreatePoolAsync(string draftPublicId)
  {
    var result = await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });
    result.IsSuccess.Should().BeTrue("CreateDraftPool must succeed");
  }

  protected async Task AddMovieToPoolAsync(string draftPublicId, int tmdbId)
  {
    var result = await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = draftPublicId,
      TmdbId = tmdbId
    });
    result.IsSuccess.Should().BeTrue($"AddMovieToPool tmdbId={tmdbId} must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Person/Drafter/Host helpers
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task<string> CreatePersonAsync(string firstName, string lastName)
  {
    var result = await Sender.Send(new CreatePersonCommand
    {
      FirstName = firstName,
      LastName = lastName,
      PublicId = $"p_{Guid.NewGuid():N}"
    });
    result.IsSuccess.Should().BeTrue($"CreatePerson {firstName} {lastName} must succeed");
    return result.Value;
  }

  protected async Task<string> CreateDrafterAsync(string personPublicId)
  {
    var result = await Sender.Send(new CreateDrafterCommand(personPublicId));
    result.IsSuccess.Should().BeTrue($"CreateDrafter person={personPublicId} must succeed");
    return result.Value;
  }

  protected async Task<string> CreateHostAsync(string personPublicId)
  {
    var result = await Sender.Send(new CreateHostCommand { PersonPublicId = personPublicId });
    result.IsSuccess.Should().BeTrue($"CreateHost person={personPublicId} must succeed");
    return result.Value;
  }

  protected async Task<string> CreateTeamAsync(string name)
  {
    var result = await Sender.Send(new CreateDrafterTeamCommand { Name = name });
    result.IsSuccess.Should().BeTrue($"CreateDrafterTeam '{name}' must succeed");
    return result.Value;
  }

  protected async Task AddDrafterToTeamAsync(string teamPublicId, string drafterPublicId)
  {
    var result = await Sender.Send(new AddDrafterToTeamCommand
    {
      DrafterTeamId = teamPublicId,
      DrafterId = drafterPublicId
    });
    result.IsSuccess.Should().BeTrue($"AddDrafterToTeam team={teamPublicId} drafter={drafterPublicId} must succeed");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Series helper
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task<Guid> CreateSeriesAsync(
    string name = "Regular",
    int continuityScope = 0 /* None */)
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = name,
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = continuityScope,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });
    return result.Value;
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Veto inventory helpers — direct DB after part-start to set rolling-in counts
  // ───────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Set rolling-in veto count for a Drafter participant AFTER the part has been started
  /// (part-start would reset them to 0 for ContinuityScope.None).
  /// Clears the EF change tracker so subsequent reads see fresh values.
  /// </summary>
  protected async Task SetRollingInVetoesAsync(
    string draftPartPublicId,
    string drafterPublicId,
    int count)
  {
    var connectionFactory = ServiceScope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync();

    await connection.ExecuteAsync(
      """
      UPDATE drafts.draft_part_participants dpp
      SET vetoes_rolling_in = @Count
      FROM drafts.draft_parts dp
      INNER JOIN drafts.drafters d ON d.id = dpp.participant_id_value
      WHERE dp.public_id = @DraftPartPublicId
        AND d.public_id = @DrafterPublicId
        AND dpp.draft_part_id = dp.id
        AND dpp.participant_kind_value = 0
      """,
      new { Count = count, DraftPartPublicId = draftPartPublicId, DrafterPublicId = drafterPublicId });

    DbContext.ChangeTracker.Clear();
  }

  /// <summary>
  /// Set rolling-in veto override count for a Drafter participant AFTER the part has been started.
  /// </summary>
  protected async Task SetRollingInVetoOverridesAsync(
    string draftPartPublicId,
    string drafterPublicId,
    int count)
  {
    var connectionFactory = ServiceScope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync();

    await connection.ExecuteAsync(
      """
      UPDATE drafts.draft_part_participants dpp
      SET veto_overrides_rolling_in = @Count
      FROM drafts.draft_parts dp
      INNER JOIN drafts.drafters d ON d.id = dpp.participant_id_value
      WHERE dp.public_id = @DraftPartPublicId
        AND d.public_id = @DrafterPublicId
        AND dpp.draft_part_id = dp.id
        AND dpp.participant_kind_value = 0
      """,
      new { Count = count, DraftPartPublicId = draftPartPublicId, DrafterPublicId = drafterPublicId });

    DbContext.ChangeTracker.Clear();
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Draft-position position publicId lookup
  // ───────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// After SetDraftPositions, look up the publicId of the position with the given name.
  /// </summary>
  protected async Task<string> GetPositionPublicIdByNameAsync(string draftPartPublicId, string positionName)
  {
    var partId = await DbContext.DraftParts
      .Where(dp => dp.PublicId == draftPartPublicId)
      .Select(dp => dp.Id)
      .FirstAsync();

    return await DbContext.DraftPositions
      .Where(pos => pos.GameBoard.DraftPartId == partId && pos.Name == positionName)
      .Select(pos => pos.PublicId)
      .FirstAsync();
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Drafter publicId lookup from Person publicId
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task<string> GetDrafterPublicIdByPersonAsync(string personPublicId)
  {
    return await DbContext.Drafters
      .AsAsyncEnumerable()
      .Where(d => d.Person.PublicId == personPublicId)
      .Select(d => d.PublicId)
      .FirstAsync();
  }

  // ───────────────────────────────────────────────────────────────────────────
  // DraftPart publicId lookup
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task<string> GetDraftPartPublicIdAsync(string draftPublicId, int partIndex = 1)
  {
    return await DbContext.Drafts
      .Where(d => d.PublicId == draftPublicId)
      .SelectMany(d => d.Parts)
      .Where(p => p.PartIndex == partIndex)
      .Select(p => p.PublicId)
      .FirstAsync();
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Movie publicId lookup by IMDb ID or title
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task<string> GetMoviePublicIdByImdbIdAsync(string imdbId)
  {
    return await DbContext.Movies
      .Where(m => m.ImdbId == imdbId)
      .Select(m => m.PublicId)
      .FirstAsync();
  }

  protected async Task<string> GetMoviePublicIdByTitleAsync(string title)
  {
    return await DbContext.Movies
      .Where(m => m.MovieTitle == title)
      .Select(m => m.PublicId)
      .FirstAsync();
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Pick assertion helpers
  // ───────────────────────────────────────────────────────────────────────────

  protected async Task AssertPickAtPositionAsync(
    string draftPartPublicId,
    int position,
    string expectedMoviePublicId)
  {
    var pick = await DbContext.Picks
      .Include(p => p.Movie)
      .FirstOrDefaultAsync(p =>
        p.DraftPart.PublicId == draftPartPublicId &&
        p.Position == position &&
        (p.Veto == null || p.Veto.IsOverridden));

    pick.Should().NotBeNull($"Expected a valid pick at position {position}");
    pick!.Movie.PublicId.Should().Be(expectedMoviePublicId,
      $"Position {position} should have movie {expectedMoviePublicId}");
  }

  protected async Task AssertPickVetoedAsync(string draftPartPublicId, int playOrder)
  {
    var pick = await DbContext.Picks
      .Include(p => p.Veto)
      .FirstAsync(p => p.DraftPart.PublicId == draftPartPublicId && p.PlayOrder == playOrder);

    pick.Veto.Should().NotBeNull($"Pick playOrder={playOrder} should be vetoed");
    pick.Veto!.IsOverridden.Should().BeFalse($"Veto on playOrder={playOrder} should not be overridden");
  }

  protected async Task AssertVetoOverriddenAsync(string draftPartPublicId, int playOrder)
  {
    var pick = await DbContext.Picks
      .Include(p => p.Veto)
        .ThenInclude(v => v!.VetoOverride)
      .FirstAsync(p => p.DraftPart.PublicId == draftPartPublicId && p.PlayOrder == playOrder);

    pick.Veto.Should().NotBeNull($"Pick playOrder={playOrder} should have a veto");
    pick.Veto!.IsOverridden.Should().BeTrue($"Veto on playOrder={playOrder} should be overridden");
    pick.Veto.VetoOverride.Should().NotBeNull($"VetoOverride record should exist on playOrder={playOrder}");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Cross-module dispatch: in-process delivery to Communications + RealTimeUpdates
  // ───────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Dispatches all integration events captured by <see cref="EventBusCapture"/> to their
  /// registered handlers in the Communications and RealTimeUpdates modules.
  /// <para>
  /// Call this after <see cref="DraftsIntegrationTest.ProcessOutboxAsync"/> to complete the
  /// full event chain: domain event → outbox → integration event captured → consumer runs →
  /// <see cref="FakeEmailCapture"/>/<see cref="DraftHubCapture"/> populated.
  /// </para>
  /// </summary>
  protected async Task DispatchIntegrationEventsAsync()
  {
    var capturedEvents = EventBusCapture.CapturedEvents.ToList();

    // Target only the two consumer modules — avoid dispatching to Drafts, Reporting, etc.
    var targetAssemblies = AppDomain.CurrentDomain.GetAssemblies()
      .Where(static a => a.GetName().Name is
        "ScreenDrafts.Modules.Communications.Features" or
        "ScreenDrafts.Modules.RealTimeUpdates.Features")
      .ToList();

    foreach (var integrationEvent in capturedEvents)
    {
      foreach (var assembly in targetAssemblies)
      {
        // Create a fresh DI scope per dispatch — mirrors production per-message scope.
        using var scope = ServiceScope.ServiceProvider.CreateScope();
        var handlers = IntegrationEventHandlersFactory.GetHandlers(
          integrationEvent.GetType(),
          scope.ServiceProvider,
          assembly);

        foreach (var handler in handlers)
        {
          await handler.Handle(integrationEvent);
        }
      }
    }
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Communications seeding helpers
  // ───────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Inserts (or upserts) a test recipient into <c>communications.user_emails</c> so that
  /// email-notification tests have at least one addressee.
  /// </summary>
  protected async Task SeedEmailRecipientAsync(
    string emailAddress,
    string fullName,
    bool isPatreon = false)
  {
    var connectionFactory = ServiceScope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync();

    await connection.ExecuteAsync(
      """
      INSERT INTO communications.user_emails (user_id, email_address, full_name, is_patreon)
      VALUES (@UserId, @EmailAddress, @FullName, @IsPatreon)
      ON CONFLICT (user_id) DO UPDATE
        SET email_address = EXCLUDED.email_address,
            full_name     = EXCLUDED.full_name,
            is_patreon    = EXCLUDED.is_patreon
      """,
      new
      {
        UserId = Guid.NewGuid(),
        EmailAddress = emailAddress,
        FullName = fullName,
        IsPatreon = isPatreon
      });
  }

  // ───────────────────────────────────────────────────────────────────────────
  // Email assertion helpers
  // ───────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Asserts that at least one email whose subject mentions <paramref name="draftTitle"/>
  /// was sent to <paramref name="recipientEmail"/>.
  /// Requires <see cref="SeedEmailRecipientAsync"/> to have been called and
  /// <see cref="DispatchIntegrationEventsAsync"/> to have delivered the events.
  /// </summary>
  protected void AssertDraftCreatedEmailSent(string draftTitle, string recipientEmail)
  {
    EmailCapture.SentEmails.Should().NotBeEmpty(
      "DispatchIntegrationEventsAsync must be called before asserting emails");

    EmailCapture.SentEmails.Should().Contain(
      e => e.ToAddress == recipientEmail &&
           e.Subject.Contains(draftTitle, StringComparison.OrdinalIgnoreCase),
      $"Expected a draft-created email for '{draftTitle}' sent to '{recipientEmail}'");
  }

  // ───────────────────────────────────────────────────────────────────────────
  // SignalR assertion helpers
  // ───────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Asserts that at least one <c>PickListUpdated</c> message was broadcast to DraftHub
  /// after <see cref="DispatchIntegrationEventsAsync"/> was called.
  /// </summary>
  protected void AssertPickListUpdatedBroadcast()
  {
    HubCapture.SentMessages.Should().Contain(
      m => m.Method == "PickListUpdated",
      "A PickAddedIntegrationEvent should trigger a PickListUpdated SignalR broadcast");
  }

  /// <summary>
  /// Asserts that <paramref name="expectedCount"/> <c>PickListUpdated</c> messages were broadcast.
  /// </summary>
  protected void AssertPickListUpdatedBroadcastCount(int expectedCount)
  {
    HubCapture.SentMessages.Count(m => m.Method == "PickListUpdated")
      .Should().Be(expectedCount,
        $"Expected exactly {expectedCount} PickListUpdated broadcast(s)");
  }
}
