using ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Scenarios;

/// <summary>
/// Scenario 6 — 80's Sports Mini-Mega Draft
///
/// Three drafters draft 12 sports films from the 1980s.
/// Drafter 1 wins trivia and gets 4 picks; Drafters 2 and 3 each get 4.
/// Uses the board (no pool). Tests veto + veto-override in 3-drafter context.
///
/// Films used (from MediaSeedFixture):
///   Personal Best (tt0084534), Raging Bull (tt0081398), Rocky III (tt0084602),
///   The Karate Kid (tt0087538), The Natural (tt0087781), Hoosiers (tt0091217),
///   Lucas (tt0091501), The Color of Money (tt0090863), Eight Men Out (tt0095016),
///   Bull Durham (tt0094812), Major League (tt0097815), Field of Dreams (tt0097351)
/// </summary>
public sealed class EightiesSportsMiniMega_Tests(DraftsIntegrationTestWebAppFactory factory)
  : DraftScenarioBase(factory)
{
  // Draft / part identifiers
  private string _draftPublicId = default!;
  private string _draftPartPublicId = default!;

  // Drafter public IDs
  private string _clayDrafterPublicId = default!;
  private string _ryanDrafterPublicId = default!;
  private string _darrenDrafterPublicId = default!;

  // Movie publicIds (12 films, keyed by position: index = position - 1)
  private string[] _moviePublicIds = default!;

  // IMDb IDs for the 12 80s sports films in order
  private static readonly string[] SportsImdbIds =
  [
    "tt0081398",  // Raging Bull (pos 1 — Clay)
    "tt0084534",  // Personal Best (pos 2 — Ryan)
    "tt0084602",  // Rocky III (pos 3 — Darren)
    "tt0087538",  // The Karate Kid (pos 4 — Clay)
    "tt0087781",  // The Natural (pos 5 — Ryan)
    "tt0091217",  // Hoosiers (pos 6 — Darren)
    "tt0091501",  // Lucas (pos 7 — Clay)
    "tt0090863",  // The Color of Money (pos 8 — Ryan)
    "tt0095016",  // Eight Men Out (pos 9 — Darren)
    "tt0094812",  // Bull Durham (pos 10 — Clay)
    "tt0097815",  // Major League (pos 11 — Ryan)
    "tt0097351",  // Field of Dreams (pos 12 — Darren)
  ];

  protected override async Task OnInitializeAsync()
  {
    await Shared.SeedAsync(Sender);
    await Media.SeedAsync(DbContext);
    EmailCapture.Clear();

    // Create drafters from the shared hosts
    _clayDrafterPublicId = await CreateDrafterAsync(Shared.ClayPersonPublicId);
    _ryanDrafterPublicId = await CreateDrafterAsync(Shared.RyanPersonPublicId);
    _darrenDrafterPublicId = await CreateDrafterAsync(Shared.DarrenPersonPublicId);

    // Step 1 — CreateDraft (Mini-Mega)
    _draftPublicId = await CreateDraftAsync(
      "80's Sports Mini-Mega Draft",
      DraftType.MiniMega.Value,
      Shared.RegularSeriesId);

    await ProcessOutboxAsync();

    // Step 2 — CreateDraftPart (1 part, 12 picks for 3 drafters: 4+4+4)
    await CreateDraftPartAsync(_draftPublicId, partIndex: 1, minPosition: 1, maxPosition: 12);
    _draftPartPublicId = await GetDraftPartPublicIdAsync(_draftPublicId);

    // Step 3 — AddParticipants
    await AddDrafterParticipantAsync(_draftPartPublicId, _clayDrafterPublicId);
    await AddDrafterParticipantAsync(_draftPartPublicId, _ryanDrafterPublicId);
    await AddDrafterParticipantAsync(_draftPartPublicId, _darrenDrafterPublicId);

    // Step 4 — AddHosts (Phil primary host, Bryan co-host)
    await AddPrimaryHostAsync(_draftPartPublicId, Shared.PhilHostPublicId);
    await AddCoHostAsync(_draftPartPublicId, Shared.BryanHostPublicId);

    // Step 5 — SetDraftPositions
    // Clay wins trivia: positions 1,4,7,10
    // Ryan 2nd: positions 2,5,8,11
    // Darren 3rd: positions 3,6,9,12
    await SetPositionsAsync(_draftPartPublicId,
    [
      new DraftPositionRequest { Name = "Clay",   Picks = [1, 4, 7, 10] },
      new DraftPositionRequest { Name = "Ryan",   Picks = [2, 5, 8, 11] },
      new DraftPositionRequest { Name = "Darren", Picks = [3, 6, 9, 12] }
    ]);

    // Step 6 — StartDraft
    await StartDraftPartAsync(_draftPublicId);

    // Step 7 — AssignTriviaResults
    await AssignTriviaAsync(_draftPartPublicId,
    [
      (_clayDrafterPublicId, ParticipantKind.Drafter, 1, 3),
      (_ryanDrafterPublicId, ParticipantKind.Drafter, 2, 2),
      (_darrenDrafterPublicId, ParticipantKind.Drafter, 3, 1)
    ]);

    // Step 8 — AssignParticipantsToPositions
    var clayPosId   = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Clay");
    var ryanPosId   = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Ryan");
    var darrenPosId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Darren");

    await AssignParticipantToPositionAsync(_draftPartPublicId, clayPosId,   _clayDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_draftPartPublicId, ryanPosId,   _ryanDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_draftPartPublicId, darrenPosId, _darrenDrafterPublicId, ParticipantKind.Drafter);

    // Collect the 12 seeded movie publicIds from MediaSeedFixture by IMDb ID
    _moviePublicIds = new string[12];
    for (var i = 0; i < 12; i++)
    {
      _moviePublicIds[i] = await GetMoviePublicIdByImdbIdAsync(SportsImdbIds[i]);
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Happy path: full 12-pick sequence
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task EightiesSports_FullPickSequence_ShouldSucceedAsync()
  {
    await PlayAllPicksAsync();

    var picks = await DbContext.Picks
      .Where(p => p.DraftPart.PublicId == _draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    picks.Should().HaveCount(12);
  }

  [Fact]
  public async Task EightiesSports_Clay_ShouldHaveFourPicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var clayId = await GetDrafterInternalIdAsync(_clayDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == clayId, TestContext.Current.CancellationToken);

    count.Should().Be(4, "Clay picks positions 1, 4, 7, 10");
  }

  [Fact]
  public async Task EightiesSports_Ryan_ShouldHaveFourPicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var ryanId = await GetDrafterInternalIdAsync(_ryanDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == ryanId, TestContext.Current.CancellationToken);

    count.Should().Be(4, "Ryan picks positions 2, 5, 8, 11");
  }

  [Fact]
  public async Task EightiesSports_Darren_ShouldHaveFourPicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var darrenId = await GetDrafterInternalIdAsync(_darrenDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == darrenId, TestContext.Current.CancellationToken);

    count.Should().Be(4, "Darren picks positions 3, 6, 9, 12");
  }

  [Fact]
  public async Task EightiesSports_EndDraft_ShouldCompleteAsync()
  {
    await PlayAllPicksAsync();
    await CompleteDraftPartAsync(_draftPublicId);

    var draftPart = await DbContext.DraftParts
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == _draftPartPublicId, TestContext.Current.CancellationToken);

    draftPart.Status.Should().Be(DraftPartStatus.Completed);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Veto and override in a 3-drafter context
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task EightiesSports_Clay_CanVeto_RyanPickAsync()
  {
    // Play positions 12 through 9
    await PlayPickAsync(_draftPartPublicId, 12, 12, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[11]);
    await PlayPickAsync(_draftPartPublicId, 11, 11, _ryanDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[10]);
    await PlayPickAsync(_draftPartPublicId, 10, 10, _clayDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[9]);

    // Ryan plays position 8
    await PlayPickAsync(_draftPartPublicId, 8, 8, _ryanDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[7]);

    // Clay vetoes Ryan's position-8 pick
    await ApplyVetoAsync(_draftPartPublicId, 8, _clayDrafterPublicId, ParticipantKind.Drafter);

    await AssertPickVetoedAsync(_draftPartPublicId, 8);
  }

  [Fact]
  public async Task EightiesSports_Darren_CanOverride_ClayVetoAsync()
  {
    // Play positions 12 through 9, veto position 8
    await PlayPickAsync(_draftPartPublicId, 12, 12, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[11]);
    await PlayPickAsync(_draftPartPublicId, 11, 11, _ryanDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[10]);
    await PlayPickAsync(_draftPartPublicId, 10, 10, _clayDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId, 8,   8, _ryanDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[7]);

    // Clay vetoes Ryan's position-8 pick
    await ApplyVetoAsync(_draftPartPublicId, 8, _clayDrafterPublicId, ParticipantKind.Drafter);

    // Darren uses his veto-override to reinstate Ryan's pick (using SetRollingInVetoOverrides to give Darren an override)
    await SetRollingInVetoOverridesAsync(_draftPartPublicId, _darrenDrafterPublicId, 1);
    await ApplyVetoOverrideAsync(_draftPartPublicId, 8, _darrenDrafterPublicId, ParticipantKind.Drafter);

    await AssertVetoOverriddenAsync(_draftPartPublicId, 8);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Guard: duplicate movie pick
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task EightiesSports_DuplicateMovie_ShouldFailAsync()
  {
    // Play position 12 with Raging Bull
    await PlayPickAsync(_draftPartPublicId, 12, 12, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[11]);

    // Try to pick the same movie at position 11
    var result = await Sender.Send(new PlayPickCommand
    {
      DraftPartId = _draftPartPublicId,
      Position = 11,
      PlayOrder = 11,
      ParticipantPublicId = _ryanDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = _moviePublicIds[11],  // same movie
      ActedByPublicId = _ryanDrafterPublicId
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue("Picking a duplicate movie in the same part must fail");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Play all 12 picks in descending order (12 → 1).
  /// Clay: 10, 7, 4, 1  |  Ryan: 11, 8, 5, 2  |  Darren: 12, 9, 6, 3
  /// </summary>
  private async Task PlayAllPicksAsync()
  {
    // Positions assigned: Clay→1,4,7,10 | Ryan→2,5,8,11 | Darren→3,6,9,12
    await PlayPickAsync(_draftPartPublicId, 12, 12, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[11]);
    await PlayPickAsync(_draftPartPublicId, 11, 11, _ryanDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[10]);
    await PlayPickAsync(_draftPartPublicId, 10, 10, _clayDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[9]);
    await PlayPickAsync(_draftPartPublicId,  9,  9, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[8]);
    await PlayPickAsync(_draftPartPublicId,  8,  8, _ryanDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[7]);
    await PlayPickAsync(_draftPartPublicId,  7,  7, _clayDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[6]);
    await PlayPickAsync(_draftPartPublicId,  6,  6, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[5]);
    await PlayPickAsync(_draftPartPublicId,  5,  5, _ryanDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[4]);
    await PlayPickAsync(_draftPartPublicId,  4,  4, _clayDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[3]);
    await PlayPickAsync(_draftPartPublicId,  3,  3, _darrenDrafterPublicId, ParticipantKind.Drafter, _moviePublicIds[2]);
    await PlayPickAsync(_draftPartPublicId,  2,  2, _ryanDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[1]);
    await PlayPickAsync(_draftPartPublicId,  1,  1, _clayDrafterPublicId,   ParticipantKind.Drafter, _moviePublicIds[0]);
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
