using ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Scenarios;

/// <summary>
/// Scenario 7 — Spielberg Produced Mega Draft (Teams format)
///
/// Three DrafterTeam participants draft 21 Spielberg-produced films.
/// Each team has two members; team A (Clay + Ryan) wins trivia and gets 7 picks.
/// Teams B (Darren + Phil) and C (Bryan solo) each get 7 picks.
/// Uses a pool. Tests DrafterTeam participant type throughout the flow.
///
/// Films: 21 Spielberg-produced films from MediaSeedFixture (no IMDb IDs — title key).
/// </summary>
public sealed class SpielbergTeamsMegaDraft_Tests(DraftsIntegrationTestWebAppFactory factory)
  : DraftScenarioBase(factory)
{
  // Draft / part identifiers
  private string _draftPublicId = default!;
  private string _draftPartPublicId = default!;

  // Team public IDs
  private string _teamAPublicId = default!;  // Clay + Ryan (trivia winner)
  private string _teamBPublicId = default!;  // Darren + Phil
  private string _teamCPublicId = default!;  // Bryan solo

  // Individual drafter public IDs (needed to add to teams)
  private string _clayDrafterPublicId = default!;
  private string _ryanDrafterPublicId = default!;
  private string _darrenDrafterPublicId = default!;
  private string _philDrafterPublicId = default!;
  private string _bryanDrafterPublicId = default!;

  // Movie publicIds indexed 0-20
  private string[] _moviePublicIds = default!;

  // Spielberg-produced titles from MediaSeedFixture (keys used there)
  private static readonly string[] SpielbergTitleKeys =
  [
    "Real Steel",
    "Jurassic Park III",
    "The Goonies",
    "The Money Pit",
    "Innerspace",
    "Arachnophobia",
    "Joe Versus the Volcano",
    "Back to the Future Part II",
    "Transformers",
    "Men in Black 3",
    "An American Tail: Fievel Goes West",
    "Letters from Iwo Jima",
    "Gremlins 2: The New Batch",
    "Poltergeist",
    "True Grit",
    "Deep Impact",
    "First Man",
    "Men in Black",
    "Twister",
    "Who Framed Roger Rabbit",
    "Back to the Future"
  ];

  protected override async Task OnInitializeAsync()
  {
    await Shared.SeedAsync(Sender);
    await Media.SeedAsync(DbContext);
    EmailCapture.Clear();

    // Create individual drafters
    _clayDrafterPublicId = await CreateDrafterAsync(Shared.ClayPersonPublicId);
    _ryanDrafterPublicId = await CreateDrafterAsync(Shared.RyanPersonPublicId);
    _darrenDrafterPublicId = await CreateDrafterAsync(Shared.DarrenPersonPublicId);
    _philDrafterPublicId = await CreateDrafterAsync(Shared.PhilPersonPublicId);
    _bryanDrafterPublicId = await CreateDrafterAsync(Shared.BryanPersonPublicId);

    // Create teams
    _teamAPublicId = await CreateTeamAsync("Team A — Clay & Ryan");
    _teamBPublicId = await CreateTeamAsync("Team B — Darren & Phil");
    _teamCPublicId = await CreateTeamAsync("Team C — Bryan");

    await AddDrafterToTeamAsync(_teamAPublicId, _clayDrafterPublicId);
    await AddDrafterToTeamAsync(_teamAPublicId, _ryanDrafterPublicId);
    await AddDrafterToTeamAsync(_teamBPublicId, _darrenDrafterPublicId);
    await AddDrafterToTeamAsync(_teamBPublicId, _philDrafterPublicId);
    await AddDrafterToTeamAsync(_teamCPublicId, _bryanDrafterPublicId);

    // Step 1 — CreateDraft (Mega)
    _draftPublicId = await CreateDraftAsync(
      "Spielberg Produced Mega Draft",
      DraftType.Mega.Value,
      Shared.RegularSeriesId);

    await ProcessOutboxAsync();

    // Step 2 — CreateDraftPart (21 picks for 3 teams: 7+7+7)
    await CreateDraftPartAsync(_draftPublicId, partIndex: 1, minPosition: 1, maxPosition: 21);
    _draftPartPublicId = await GetDraftPartPublicIdAsync(_draftPublicId);

    // Step 3 — AddParticipants (DrafterTeam kind)
    await AddTeamParticipantAsync(_draftPartPublicId, _teamAPublicId);
    await AddTeamParticipantAsync(_draftPartPublicId, _teamBPublicId);
    await AddTeamParticipantAsync(_draftPartPublicId, _teamCPublicId);

    // Step 4 — AddHosts
    await AddPrimaryHostAsync(_draftPartPublicId, Shared.ClayHostPublicId);
    await AddCoHostAsync(_draftPartPublicId, Shared.RyanHostPublicId);

    // Step 5 — SetDraftPositions
    // Team A wins trivia: positions 1,4,7,10,13,16,19 (7 picks)
    // Team B: positions 2,5,8,11,14,17,20 (7 picks)
    // Team C: positions 3,6,9,12,15,18,21 (7 picks)
    await SetPositionsAsync(_draftPartPublicId,
    [
      new DraftPositionRequest { Name = "Team A", Picks = [1, 4, 7, 10, 13, 16, 19] },
      new DraftPositionRequest { Name = "Team B", Picks = [2, 5, 8, 11, 14, 17, 20] },
      new DraftPositionRequest { Name = "Team C", Picks = [3, 6, 9, 12, 15, 18, 21] }
    ]);

    // Step 6 — StartDraft
    await StartDraftPartAsync(_draftPublicId);

    // Step 7 — AssignTriviaResults (teams participate in trivia)
    await AssignTriviaAsync(_draftPartPublicId,
    [
      (_teamAPublicId, ParticipantKind.Team, 1, 5),
      (_teamBPublicId, ParticipantKind.Team, 2, 3),
      (_teamCPublicId, ParticipantKind.Team, 3, 1)
    ]);

    // Step 8 — AssignParticipantsToPositions
    var teamAPosId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Team A");
    var teamBPosId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Team B");
    var teamCPosId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Team C");

    await AssignParticipantToPositionAsync(_draftPartPublicId, teamAPosId, _teamAPublicId, ParticipantKind.Team);
    await AssignParticipantToPositionAsync(_draftPartPublicId, teamBPosId, _teamBPublicId, ParticipantKind.Team);
    await AssignParticipantToPositionAsync(_draftPartPublicId, teamCPosId, _teamCPublicId, ParticipantKind.Team);

    // Seed 21 Spielberg films (fictional TmdbIds 50001-50021)
    _moviePublicIds = new string[21];
    for (var i = 0; i < 21; i++)
    {
      var publicId = $"m_{Guid.NewGuid():N}";
      var movie = Movie.Create(
        SpielbergTitleKeys[i],
        publicId,
        MediaType.Movie,
        Guid.NewGuid(),
        tmdbId: 50001 + i).Value;
      DbContext.Movies.Add(movie);
      _moviePublicIds[i] = publicId;
    }

    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Create pool and add all 21 films
    await CreatePoolAsync(_draftPublicId);
    for (var i = 0; i < 21; i++)
    {
      await AddMovieToPoolAsync(_draftPublicId, 50001 + i);
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Happy path: full 21-pick sequence with DrafterTeam participants
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SpielbergTeams_FullPickSequence_ShouldSucceedAsync()
  {
    await PlayAllPicksAsync();

    var picks = await DbContext.Picks
      .Where(p => p.DraftPart.PublicId == _draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    picks.Should().HaveCount(21);
  }

  [Fact]
  public async Task SpielbergTeams_AllPicksShouldBePlayedByDrafterTeamKindAsync()
  {
    await PlayAllPicksAsync();

    var nonTeamPicks = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue != ParticipantKind.Team, TestContext.Current.CancellationToken);

    nonTeamPicks.Should().Be(0, "All picks should be attributed to DrafterTeam participants");
  }

  [Fact]
  public async Task SpielbergTeams_EachTeam_ShouldHaveSevenPicksAsync()
  {
    await PlayAllPicksAsync();

    var teamAId = await GetTeamInternalIdAsync(_teamAPublicId);
    var teamBId = await GetTeamInternalIdAsync(_teamBPublicId);
    var teamCId = await GetTeamInternalIdAsync(_teamCPublicId);

    var countA = await DbContext.Picks.CountAsync(p =>
      p.DraftPart.PublicId == _draftPartPublicId &&
      p.PlayedByParticipantKindValue == ParticipantKind.Team &&
      p.PlayedByParticipantIdValue == teamAId, TestContext.Current.CancellationToken);

    var countB = await DbContext.Picks.CountAsync(p =>
      p.DraftPart.PublicId == _draftPartPublicId &&
      p.PlayedByParticipantKindValue == ParticipantKind.Team &&
      p.PlayedByParticipantIdValue == teamBId, TestContext.Current.CancellationToken);

    var countC = await DbContext.Picks.CountAsync(p =>
      p.DraftPart.PublicId == _draftPartPublicId &&
      p.PlayedByParticipantKindValue == ParticipantKind.Team &&
      p.PlayedByParticipantIdValue == teamCId, TestContext.Current.CancellationToken);

    countA.Should().Be(7, "Team A should have 7 picks");
    countB.Should().Be(7, "Team B should have 7 picks");
    countC.Should().Be(7, "Team C should have 7 picks");
  }

  [Fact]
  public async Task SpielbergTeams_EndDraft_ShouldCompleteAsync()
  {
    await PlayAllPicksAsync();
    await CompleteDraftPartAsync(_draftPublicId);

    var draftPart = await DbContext.DraftParts
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == _draftPartPublicId, TestContext.Current.CancellationToken);

    draftPart.Status.Should().Be(DraftPartStatus.Completed);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Veto test: Team A vetoes Team B's pick
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SpielbergTeams_TeamA_CanVeto_TeamBPickAsync()
  {
    // Play positions 21 down to 16
    for (var i = 20; i >= 15; i--)
    {
      var pos = i + 1;
      var (team, _) = TeamForPosition(pos);
      await PlayPickAsync(_draftPartPublicId, pos, pos, team, ParticipantKind.Team, _moviePublicIds[i]);
    }

    // Team B plays position 14
    await PlayPickAsync(_draftPartPublicId, 14, 14, _teamBPublicId, ParticipantKind.Team, _moviePublicIds[13]);

    // Team A vetoes Team B's position-14 pick
    await ApplyVetoAsync(_draftPartPublicId, 14, _teamAPublicId, ParticipantKind.Team);

    await AssertPickVetoedAsync(_draftPartPublicId, 14);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Guard: cannot add drafter directly (must use team)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SpielbergTeams_AddDrafterDirectly_ShouldFailAsync()
  {
    // Arrange: a separate draft part with team participants
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync("Team Guard Test", DraftType.Mega.Value, seriesId);
    await CreateDraftPartAsync(draftPublicId, 1, 1, 7);
    var partPublicId = await GetDraftPartPublicIdAsync(draftPublicId);

    // Two team participants are required (Start() needs ≥2 participants) and a primary host.
    await AddTeamParticipantAsync(partPublicId, _teamAPublicId);
    await AddTeamParticipantAsync(partPublicId, _teamBPublicId);
    await AddPrimaryHostAsync(partPublicId, Shared.PhilHostPublicId);

    var movie = Movie.Create("Guard Test Movie", $"m_{Guid.NewGuid():N}", MediaType.Movie, Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Start the draft first (needs positions — one per team participant)
    await SetPositionsAsync(partPublicId,
    [
      new DraftPositionRequest { Name = "Team A", Picks = [1, 3, 5, 7] },
      new DraftPositionRequest { Name = "Team B", Picks = [2, 4, 6] }
    ]);
    await StartDraftPartAsync(draftPublicId);

    // Attempt to play a pick using a Drafter (not a team)
    var result = await Sender.Send(new PlayPickCommand
    {
      DraftPartId = partPublicId,
      Position = 7,
      PlayOrder = 7,
      ParticipantPublicId = _clayDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId,
      ActedByPublicId = _clayDrafterPublicId
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue("Cannot pick as individual Drafter when only team participants are registered");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Play all 21 picks descending (21 → 1).
  /// Team A→1,4,7,10,13,16,19 | Team B→2,5,8,11,14,17,20 | Team C→3,6,9,12,15,18,21
  /// </summary>
  private async Task PlayAllPicksAsync()
  {
    for (var pos = 21; pos >= 1; pos--)
    {
      var movieIdx = pos - 1;
      var (team, _) = TeamForPosition(pos);
      await PlayPickAsync(_draftPartPublicId, pos, pos, team, ParticipantKind.Team, _moviePublicIds[movieIdx]);
    }
  }

  /// <summary>Returns (teamPublicId, movieIndex) for the given position.</summary>
  private (string teamPublicId, int movieIdx) TeamForPosition(int position)
  {
    return (position % 3) switch
    {
      1 => (_teamAPublicId, position - 1),   // positions 1,4,7,10,13,16,19
      2 => (_teamBPublicId, position - 1),   // positions 2,5,8,11,14,17,20
      0 => (_teamCPublicId, position - 1),   // positions 3,6,9,12,15,18,21
      _ => throw new InvalidOperationException()
    };
  }

  private async Task<Guid> GetTeamInternalIdAsync(string teamPublicId)
  {
    var id = await DbContext.DrafterTeams
      .Where(t => t.PublicId == teamPublicId)
      .Select(t => t.Id)
      .FirstAsync(TestContext.Current.CancellationToken);
    return id.Value;
  }
}
