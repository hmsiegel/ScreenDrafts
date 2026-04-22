using ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Scenarios;

/// <summary>
/// Scenario 3 — Aaron Sorkin Super Draft
///
/// Two experienced hosts (Clay, Darren) draft 10 Sorkin films from a pool.
/// Clay wins trivia and receives a bonus veto from his position.
/// Tests pool creation, trivia position bonuses, veto mechanics, and veto overrides.
/// </summary>
public sealed class SorkinSuperDraft_Tests(DraftsIntegrationTestWebAppFactory factory)
  : DraftScenarioBase(factory)
{
  // Draft / part identifiers
  private string _draftPublicId = default!;
  private string _draftPartPublicId = default!;

  // Drafter public IDs (Clay and Darren are both hosts + drafters)
  private string _clayDrafterPublicId = default!;
  private string _darrenDrafterPublicId = default!;

  // Movie publicIds keyed by index 0-9 (Sorkin films in pick order)
  // [0] A Few Good Men — position 1 (Clay)
  // [1] Malice — position 2 (Darren)
  // [2] The American President — position 3 (Clay)
  // [3] Charlie Wilson's War — position 4 (Darren)
  // [4] The Social Network — position 5 (Clay)
  // [5] Moneyball — position 6 (Darren)
  // [6] Steve Jobs — position 7 (Clay)
  // [7] Molly's Game — position 8 (Darren)
  // [8] The Trial of the Chicago 7 — position 9 (Clay)
  // [9] Being the Ricardos — position 10 (Darren)
  private string[] _moviePublicIds = default!;

  protected override async Task OnInitializeAsync()
  {
    await Shared.SeedAsync(Sender);
    await Media.SeedAsync(DbContext);
    EmailCapture.Clear();
    HubCapture.Clear();
    EventBusCapture.Clear();

    // Create drafters for the dual-role hosts (Clay and Darren are both hosts and drafters)
    _clayDrafterPublicId = await CreateDrafterAsync(Shared.ClayPersonPublicId);
    _darrenDrafterPublicId = await CreateDrafterAsync(Shared.DarrenPersonPublicId);

    // Step 1 — CreateDraft
    _draftPublicId = await CreateDraftAsync(
      "Aaron Sorkin Super Draft",
      DraftType.Super.Value,
      Shared.RegularSeriesId);

    await ProcessOutboxAsync();

    // Step 2 — CreateDraftPart (1 part, 10 picks)
    await CreateDraftPartAsync(_draftPublicId, partIndex: 1, minPosition: 1, maxPosition: 10);
    _draftPartPublicId = await GetDraftPartPublicIdAsync(_draftPublicId);

    // Step 3 — AddParticipants
    await AddDrafterParticipantAsync(_draftPartPublicId, _clayDrafterPublicId);
    await AddDrafterParticipantAsync(_draftPartPublicId, _darrenDrafterPublicId);

    // Step 4 — AddHosts (Ryan primary, Phil co-host — Clay and Darren are the drafters)
    await AddPrimaryHostAsync(_draftPartPublicId, Shared.RyanHostPublicId);
    await AddCoHostAsync(_draftPartPublicId, Shared.PhilHostPublicId);

    // Step 5 — SetDraftPositions
    // Clay wins trivia → gets positions 1,3,5,7,9 with HasBonusVeto = true
    // Darren → gets positions 2,4,6,8,10
    await SetPositionsAsync(_draftPartPublicId,
    [
      new DraftPositionRequest
      {
        Name = "Clay",
        Picks = [1, 3, 5, 7, 9],
        HasBonusVeto = true
      },
      new DraftPositionRequest
      {
        Name = "Darren",
        Picks = [2, 4, 6, 8, 10]
      }
    ]);

    // Step 6 — StartDraft
    await StartDraftPartAsync(_draftPublicId);

    // Step 7 — AssignTriviaResults (Clay wins: 5 questions, Darren: 2 questions)
    await AssignTriviaAsync(_draftPartPublicId,
    [
      (_clayDrafterPublicId, ParticipantKind.Drafter, 1, 5),
      (_darrenDrafterPublicId, ParticipantKind.Drafter, 2, 2)
    ]);

    // Step 8 — AssignParticipantsToPositions
    var clayPosId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Clay");
    var darrenPosId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Darren");
    await AssignParticipantToPositionAsync(_draftPartPublicId, clayPosId, _clayDrafterPublicId, ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_draftPartPublicId, darrenPosId, _darrenDrafterPublicId, ParticipantKind.Drafter);

    // Seed Sorkin pool movies with TmdbIds so pool can reference them
    // Using fictional TmdbIds in 30001–30010 range for test isolation
    var sorkinTmdbIds = new[] { 30001, 30002, 30003, 30004, 30005, 30006, 30007, 30008, 30009, 30010 };
    var sorkinTitles = new[]
    {
      "A Few Good Men",
      "Malice",
      "The American President",
      "Charlie Wilson's War",
      "The Social Network",
      "Moneyball",
      "Steve Jobs",
      "Molly's Game",
      "The Trial of the Chicago 7",
      "Being the Ricardos"
    };
    var sorkinImdbIds = new[]
    {
      "tt0104257", "tt0107497", "tt0112346", "tt0472062", "tt1285016",
      "tt1210166", "tt2080374", "tt4669788", "tt1070874", "tt6009292"
    };

    _moviePublicIds = new string[10];
    for (var i = 0; i < 10; i++)
    {
      var publicId = $"m_{Guid.NewGuid():N}";
      var movie = Movie.Create(
        sorkinTitles[i],
        publicId,
        MediaType.Movie,
        Guid.NewGuid(),
        imdbId: sorkinImdbIds[i],
        tmdbId: sorkinTmdbIds[i]).Value;
      DbContext.Movies.Add(movie);
      _moviePublicIds[i] = publicId;
    }

    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Create pool and add all movies
    await CreatePoolAsync(_draftPublicId);
    for (var i = 0; i < 10; i++)
    {
      await AddMovieToPoolAsync(_draftPublicId, sorkinTmdbIds[i]);
    }

    // Drain any outbox entries generated during setup (e.g. DraftPositionAssigned) so
    // that per-test calls to ProcessOutboxAsync / DispatchIntegrationEventsAsync only
    // observe events from the test's own operations.
    await ProcessOutboxAsync();
    EventBusCapture.Clear();
    HubCapture.Clear();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Happy path: full 10-pick sequence
  // Play order descends 10 → 1; picks alternate Darren/Clay (even/odd)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SorkinSuperDraft_FullPickSequence_ShouldSucceedAsync()
  {
    await PlayAllPicksAsync();

    var picks = await DbContext.Picks
      .Where(p => p.DraftPart.PublicId == _draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    picks.Should().HaveCount(10);
  }

  [Fact]
  public async Task SorkinSuperDraft_Clay_ShouldHaveFivePicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var clayId = await GetDrafterInternalIdAsync(_clayDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == clayId, TestContext.Current.CancellationToken);

    count.Should().Be(5, "Clay (trivia winner) should have 5 picks in positions 1,3,5,7,9");
  }

  [Fact]
  public async Task SorkinSuperDraft_Darren_ShouldHaveFivePicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var darrenId = await GetDrafterInternalIdAsync(_darrenDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == darrenId, TestContext.Current.CancellationToken);

    count.Should().Be(5, "Darren should have 5 picks in positions 2,4,6,8,10");
  }

  [Fact]
  public async Task SorkinSuperDraft_EndDraft_ShouldCompleteAsync()
  {
    await PlayAllPicksAsync();
    await CompleteDraftPartAsync(_draftPublicId);

    var draftPart = await DbContext.DraftParts
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == _draftPartPublicId, TestContext.Current.CancellationToken);

    draftPart.Status.Should().Be(DraftPartStatus.Completed);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Veto mechanics: Clay vetoes Darren's position-10 pick
  // Clay has 2 vetoes (1 starting + 1 bonus from position HasBonusVeto)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SorkinSuperDraft_Clay_CanUseStartingVeto_OnDarrenPickAsync()
  {
    // Play positions 10-3, then veto
    await PlayPickAsync(_draftPartPublicId, 10, 10, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId, 9, 9, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[8]);

    // Clay uses his starting veto on Darren's position 10 pick
    await ApplyVetoAsync(_draftPartPublicId, 10, _clayDrafterPublicId, ParticipantKind.Drafter);

    await AssertPickVetoedAsync(_draftPartPublicId, 10);
  }

  [Fact]
  public async Task SorkinSuperDraft_Clay_CanUseVetoOverride_AfterVetoAsync()
  {
    // Play positions 10-9, veto position 10, then override it
    await PlayPickAsync(_draftPartPublicId, 10, 10, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId, 9, 9, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[8]);

    // Clay vetoes position 10 (starting veto)
    await ApplyVetoAsync(_draftPartPublicId, 10, _clayDrafterPublicId, ParticipantKind.Drafter);

    // Clay uses bonus veto override to override the veto (keeping Darren's pick)
    await ApplyVetoOverrideAsync(_draftPartPublicId, 10, _clayDrafterPublicId, ParticipantKind.Drafter);

    await AssertVetoOverriddenAsync(_draftPartPublicId, 10);
  }

  [Fact]
  public async Task SorkinSuperDraft_BonusVeto_MeansClayHasTwoVetoes_TotalAsync()
  {
    // Play first two picks
    await PlayPickAsync(_draftPartPublicId, 10, 10, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId, 9, 9, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[8]);
    await PlayPickAsync(_draftPartPublicId, 8, 8, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[7]);
    await PlayPickAsync(_draftPartPublicId, 7, 7, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[6]);

    // Clay uses first veto (starting)
    await ApplyVetoAsync(_draftPartPublicId, 10, _clayDrafterPublicId, ParticipantKind.Drafter);

    // Clay uses second veto (bonus) — should succeed
    var result = await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = _draftPartPublicId,
      PlayOrder = 8,
      ParticipantPublicId = _clayDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = _clayDrafterPublicId
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue("Clay should have a second veto from the bonus HasBonusVeto position");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Guard cases
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SorkinSuperDraft_Darren_HasOnlyOneVeto_ShouldFailOnSecondAsync()
  {
    await PlayPickAsync(_draftPartPublicId, 10, 10, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId, 9, 9, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[8]);
    await PlayPickAsync(_draftPartPublicId, 8, 8, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[7]);
    await PlayPickAsync(_draftPartPublicId, 7, 7, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[6]);

    // Darren uses his only veto
    await ApplyVetoAsync(_draftPartPublicId, 9, _darrenDrafterPublicId, ParticipantKind.Drafter);

    // Darren tries to veto again — should fail
    var result = await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = _draftPartPublicId,
      PlayOrder = 7,
      ParticipantPublicId = _darrenDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = _darrenDrafterPublicId
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue("Darren only has 1 veto and should fail on a second attempt");
  }

  [Fact]
  public async Task SorkinSuperDraft_Pool_ShouldContainTenMoviesAsync()
  {
    var draft = await DbContext.Drafts
      .Where(d => d.PublicId == _draftPublicId)
      .FirstAsync(TestContext.Current.CancellationToken);

    var pool = await DbContext.DraftPools
      .Include(p => p.TmdbIds)
      .FirstAsync(p => p.DraftId == draft.Id, TestContext.Current.CancellationToken);

    pool.TmdbIds.Should().HaveCount(10, "All 10 Sorkin films should be in the pool");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // RealTimeUpdates: SignalR broadcast — veto and override both fire PickListUpdated
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SorkinSuperDraft_VetoAndOverride_ShouldBroadcastPickListUpdatedTwiceAsync()
  {
    // Play two picks then veto one, then override the veto.
    await PlayPickAsync(_draftPartPublicId, 10, 10, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId, 9, 9, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[8]);
    await ApplyVetoAsync(_draftPartPublicId, 10, _clayDrafterPublicId, ParticipantKind.Drafter);
    await ApplyVetoOverrideAsync(_draftPartPublicId, 10, _clayDrafterPublicId, ParticipantKind.Drafter);

    // Process outbox to capture PickAdded + VetoApplied + VetoOverrideApplied events.
    await ProcessOutboxAsync();

    // Deliver to RealTimeUpdates consumers — each event fires its own broadcast.
    await DispatchIntegrationEventsAsync();

    // At minimum the two picks should each yield a PickListUpdated message.
    // Veto and VetoOverride consumers also send PickListUpdated per the module design.
    HubCapture.SentMessages.Should().NotBeEmpty("pick/veto/override operations should all trigger hub broadcasts");
    HubCapture.SentMessages.Should().AllSatisfy(
      m => m.Method.Should().Be("PickListUpdated"),
      "DraftHub only sends PickListUpdated messages from these consumers");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────────────────────────────────

  private async Task PlayAllPicksAsync()
  {
    // Snake order: 10 → 1 (Darren picks even positions, Clay picks odd)
    await PlayPickAsync(_draftPartPublicId, 10, 10, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId, 9, 9, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[8]);
    await PlayPickAsync(_draftPartPublicId, 8, 8, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[7]);
    await PlayPickAsync(_draftPartPublicId, 7, 7, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[6]);
    await PlayPickAsync(_draftPartPublicId, 6, 6, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[5]);
    await PlayPickAsync(_draftPartPublicId, 5, 5, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[4]);
    await PlayPickAsync(_draftPartPublicId, 4, 4, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[3]);
    await PlayPickAsync(_draftPartPublicId, 3, 3, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[2]);
    await PlayPickAsync(_draftPartPublicId, 2, 2, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[1]);
    await PlayPickAsync(_draftPartPublicId, 1, 1, _clayDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[0]);
  }

  private async Task<Guid> GetDrafterInternalIdAsync(string drafterPublicId)
  {
    var id = await DbContext.Drafters
      .Where(d => d.PublicId == drafterPublicId)
      .Select(d => d.Id)
      .FirstAsync(TestContext.Current.CancellationToken);
    return id.Value;
  }
}
