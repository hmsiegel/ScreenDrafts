using ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Scenarios;

/// <summary>
/// Scenario 4 — Martin Scorsese Super Draft (Multi-Part)
///
/// A multi-part Super draft of Scorsese's filmography.
/// Part 1: Clay, Ryan, Darren, Phil draft 12 early films (1967-1982).
///         Community participant joins — can veto.
///         Rolling-in vetoes carry to Part 2.
/// Part 2: Same drafters + community pick up from Part 2 films (1983-1995).
///         Darren picks "The Departed" at position 10 (carry-forward test).
/// Part 3: Clay, Ryan, Darren, Phil, Bryan draft films from 1995-2023.
///         Bryan is dual-role (host + drafter).
///         Community veto override applied.
///
/// This test file covers the full 3-part flow plus individual part guard tests.
/// </summary>
public sealed class ScorseseSuperDraft_Tests(DraftsIntegrationTestWebAppFactory factory)
  : DraftScenarioBase(factory)
{
  // Draft identifiers
  private string _draftPublicId = default!;
  private string _part1PublicId = default!;
  private string _part2PublicId = default!;
  private string _part3PublicId = default!;

  // Drafter public IDs
  private string _clayDrafterPublicId = default!;
  private string _ryanDrafterPublicId = default!;
  private string _darrenDrafterPublicId = default!;
  private string _philDrafterPublicId = default!;
  private string _bryanDrafterPublicId = default!;

  // Part 1 movies (12 Scorsese early films)
  private static readonly string[] Part1ImdbIds =
  [
    "tt0063876",  // Who's That Knocking at My Door
    "tt0068232",  // Boxcar Bertha
    "tt0070379",  // Mean Streets
    "tt0071115",  // Alice Doesn't Live Here Anymore
    "tt0075314",  // Taxi Driver
    "tt0076451",  // New York, New York
    "tt0077838",  // The Last Waltz
    "tt0081398",  // Raging Bull
    "tt0085794",  // The King of Comedy
    "tt0088680",  // After Hours
    "tt0090863",  // The Color of Money
    "tt0095497",  // The Last Temptation of Christ
  ];

  // Part 2 movies (10 Scorsese mid-career films)
  private static readonly string[] Part2ImdbIds =
  [
    "tt0099685",  // Goodfellas
    "tt0101540",  // Cape Fear
    "tt0106226",  // The Age of Innocence
    "tt0112641",  // Casino
    "tt0119485",  // Kundun
    "tt0163988",  // Bringing Out the Dead
    "tt0217505",  // Gangs of New York
    "tt0338751",  // The Aviator
    "tt0407887",  // The Departed
    "tt1130884",  // Shutter Island
  ];

  // Part 3 movies (8 Scorsese late-career films)
  private static readonly string[] Part3ImdbIds =
  [
    "tt0970179",  // Hugo
    "tt0993846",  // The Wolf of Wall Street
    "tt0490215",  // Silence
    "tt1302006",  // The Irishman
    "tt0367631",  // No Direction Home
    "tt5537002",  // Killers of the Flower Moon
    "tt0893382",  // Shine a Light
    "tt10293552", // Rolling Thunder Revue
  ];

  private string[] _part1MoviePublicIds = default!;
  private string[] _part2MoviePublicIds = default!;
  private string[] _part3MoviePublicIds = default!;

  protected override async Task OnInitializeAsync()
  {
    await Shared.SeedAsync(Sender);
    await Media.SeedAsync(DbContext);
    EmailCapture.Clear();

    // Create drafters
    _clayDrafterPublicId   = await CreateDrafterAsync(Shared.ClayPersonPublicId);
    _ryanDrafterPublicId   = await CreateDrafterAsync(Shared.RyanPersonPublicId);
    _darrenDrafterPublicId = await CreateDrafterAsync(Shared.DarrenPersonPublicId);
    _philDrafterPublicId   = await CreateDrafterAsync(Shared.PhilPersonPublicId);
    _bryanDrafterPublicId  = await CreateDrafterAsync(Shared.BryanPersonPublicId);

    // Step 1 — CreateDraft
    _draftPublicId = await CreateDraftAsync(
      "Martin Scorsese Super Draft",
      DraftType.Super.Value,
      Shared.RegularSeriesId);

    await ProcessOutboxAsync();

    // Create Part 1 (12 picks)
    await CreateDraftPartAsync(_draftPublicId, 1, 1, 12);
    _part1PublicId = await GetDraftPartPublicIdAsync(_draftPublicId, 1);

    // Create Part 2 (10 picks)
    await CreateDraftPartAsync(_draftPublicId, 2, 1, 10);
    _part2PublicId = await GetDraftPartPublicIdAsync(_draftPublicId, 2);

    // Create Part 3 (8 picks)
    await CreateDraftPartAsync(_draftPublicId, 3, 1, 8);
    _part3PublicId = await GetDraftPartPublicIdAsync(_draftPublicId, 3);

    // Collect movie publicIds for all 3 parts
    _part1MoviePublicIds = await GetMoviePublicIdsAsync(Part1ImdbIds);
    _part2MoviePublicIds = await GetMoviePublicIdsAsync(Part2ImdbIds);
    _part3MoviePublicIds = await GetMoviePublicIdsAsync(Part3ImdbIds);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Part 1: Early Scorsese (12 films, 4 drafters + Community)
  // Positions: Clay→1,5,9 | Ryan→2,6,10 | Darren→3,7,11 | Phil→4,8,12
  // Community participant included but has no positions (vetoes only)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ScorsesePart1_FullPickSequence_ShouldSucceedAsync()
  {
    await SetupPart1Async();

    // Play all 12 picks
    await PlayPart1PicksAsync();

    var picks = await DbContext.Picks
      .Where(p => p.DraftPart.PublicId == _part1PublicId)
      .ToListAsync();

    picks.Should().HaveCount(12);
  }

  [Fact]
  public async Task ScorsesePart1_CommunityVeto_ShouldSucceedAsync()
  {
    await SetupPart1Async();

    // Play position 12 through 10
    await PlayPickAsync(_part1PublicId, 12, 12, _philDrafterPublicId, ParticipantKind.Drafter, _part1MoviePublicIds[11]);
    await PlayPickAsync(_part1PublicId, 11, 11, _darrenDrafterPublicId, ParticipantKind.Drafter, _part1MoviePublicIds[10]);
    await PlayPickAsync(_part1PublicId, 10, 10, _ryanDrafterPublicId, ParticipantKind.Drafter, _part1MoviePublicIds[9]);

    // Community vetoes Ryan's position-10 pick
    await ApplyCommunityVetoAsync(_part1PublicId, 10);

    await AssertPickVetoedAsync(_part1PublicId, 10);
  }

  [Fact]
  public async Task ScorsesePart1_CompletesPart1_StatusIsCompletedAsync()
  {
    await SetupPart1Async();
    await PlayPart1PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 1);

    var part = await DbContext.DraftParts
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == _part1PublicId);

    part.Status.Should().Be(DraftPartStatus.Completed);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Part 2: Mid-Career Scorsese (10 films, same 4 drafters)
  // Rolling-in vetoes from Part 1 can carry to Part 2 (via direct SQL)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ScorsesePart2_FullPickSequence_ShouldSucceedAsync()
  {
    await SetupPart1Async();
    await PlayPart1PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 1);

    await SetupPart2Async();
    await PlayPart2PicksAsync();

    var picks = await DbContext.Picks
      .Where(p => p.DraftPart.PublicId == _part2PublicId)
      .ToListAsync();

    picks.Should().HaveCount(10);
  }

  [Fact]
  public async Task ScorsesePart2_WithRollingInVetoes_ClayCanVetoPart2PickAsync()
  {
    await SetupPart1Async();
    await PlayPart1PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 1);

    await SetupPart2Async();

    // Give Clay a rolling-in veto (simulate carry-forward from Part 1)
    await SetRollingInVetoesAsync(_part2PublicId, _clayDrafterPublicId, 1);

    // Play positions 10 and 9
    await PlayPickAsync(_part2PublicId, 10, 10, _ryanDrafterPublicId, ParticipantKind.Drafter, _part2MoviePublicIds[9]);
    await PlayPickAsync(_part2PublicId, 9,   9, _darrenDrafterPublicId, ParticipantKind.Drafter, _part2MoviePublicIds[8]);

    // Clay vetoes Ryan's position-10 pick using his rolling-in veto
    await ApplyVetoAsync(_part2PublicId, 10, _clayDrafterPublicId, ParticipantKind.Drafter);

    await AssertPickVetoedAsync(_part2PublicId, 10);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Part 3: Late-Career Scorsese (8 films, 5 drafters — Bryan joins as dual-role)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ScorsesePart3_WithBryan_FullPickSequence_ShouldSucceedAsync()
  {
    await SetupPart1Async();
    await PlayPart1PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 1);

    await SetupPart2Async();
    await PlayPart2PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 2);

    await SetupPart3Async();
    await PlayPart3PicksAsync();

    var picks = await DbContext.Picks
      .Where(p => p.DraftPart.PublicId == _part3PublicId)
      .ToListAsync();

    picks.Should().HaveCount(8);
  }

  [Fact]
  public async Task ScorsesePart3_CommunityVetoOverride_ShouldSucceedAsync()
  {
    await SetupPart1Async();
    await PlayPart1PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 1);

    await SetupPart2Async();
    await PlayPart2PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 2);

    await SetupPart3Async();

    // Play positions 8-7
    await PlayPickAsync(_part3PublicId, 8, 8, _philDrafterPublicId, ParticipantKind.Drafter, _part3MoviePublicIds[7]);
    await PlayPickAsync(_part3PublicId, 7, 7, _darrenDrafterPublicId, ParticipantKind.Drafter, _part3MoviePublicIds[6]);

    // Community vetoes position-8 pick
    await ApplyCommunityVetoAsync(_part3PublicId, 8);

    // Give Clay a veto override
    await SetRollingInVetoOverridesAsync(_part3PublicId, _clayDrafterPublicId, 1);

    // Clay overrides the community veto
    await ApplyVetoOverrideAsync(_part3PublicId, 8, _clayDrafterPublicId, ParticipantKind.Drafter);

    await AssertVetoOverriddenAsync(_part3PublicId, 8);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Full 3-part flow
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ScorseseSuperDraft_FullThreePartFlow_ShouldCompleteAsync()
  {
    // Part 1
    await SetupPart1Async();
    await PlayPart1PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 1);

    // Part 2
    await SetupPart2Async();
    await PlayPart2PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 2);

    // Part 3
    await SetupPart3Async();
    await PlayPart3PicksAsync();
    await CompleteDraftPartAsync(_draftPublicId, 3);

    // Assert all three parts are completed
    var part1 = await DbContext.DraftParts.AsNoTracking().FirstAsync(dp => dp.PublicId == _part1PublicId);
    var part2 = await DbContext.DraftParts.AsNoTracking().FirstAsync(dp => dp.PublicId == _part2PublicId);
    var part3 = await DbContext.DraftParts.AsNoTracking().FirstAsync(dp => dp.PublicId == _part3PublicId);

    part1.Status.Should().Be(DraftPartStatus.Completed, "Part 1 should be completed");
    part2.Status.Should().Be(DraftPartStatus.Completed, "Part 2 should be completed");
    part3.Status.Should().Be(DraftPartStatus.Completed, "Part 3 should be completed");

    var part1Count = await DbContext.Picks.CountAsync(p => p.DraftPart.PublicId == _part1PublicId);
    var part2Count = await DbContext.Picks.CountAsync(p => p.DraftPart.PublicId == _part2PublicId);
    var part3Count = await DbContext.Picks.CountAsync(p => p.DraftPart.PublicId == _part3PublicId);

    (part1Count + part2Count + part3Count).Should().Be(30, "12 + 10 + 8 picks across 3 parts");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Setup helpers (per-part)
  // ─────────────────────────────────────────────────────────────────────────

  private async Task SetupPart1Async()
  {
    // Participants: 4 drafters + Community
    await AddDrafterParticipantAsync(_part1PublicId, _clayDrafterPublicId);
    await AddDrafterParticipantAsync(_part1PublicId, _ryanDrafterPublicId);
    await AddDrafterParticipantAsync(_part1PublicId, _darrenDrafterPublicId);
    await AddDrafterParticipantAsync(_part1PublicId, _philDrafterPublicId);
    await AddCommunityParticipantAsync(_part1PublicId);

    await AddPrimaryHostAsync(_part1PublicId, Shared.ClayHostPublicId);
    await AddCoHostAsync(_part1PublicId, Shared.RyanHostPublicId);

    // Positions: 4 drafters × 3 picks each = 12
    await SetPositionsAsync(_part1PublicId,
    [
      new DraftPositionRequest { Name = "Clay",   Picks = [1, 5, 9] },
      new DraftPositionRequest { Name = "Ryan",   Picks = [2, 6, 10] },
      new DraftPositionRequest { Name = "Darren", Picks = [3, 7, 11] },
      new DraftPositionRequest { Name = "Phil",   Picks = [4, 8, 12] }
    ]);

    await StartDraftPartAsync(_draftPublicId, 1);

    await AssignTriviaAsync(_part1PublicId,
    [
      (_clayDrafterPublicId,   ParticipantKind.Drafter, 1, 4),
      (_ryanDrafterPublicId,   ParticipantKind.Drafter, 2, 3),
      (_darrenDrafterPublicId, ParticipantKind.Drafter, 3, 2),
      (_philDrafterPublicId,   ParticipantKind.Drafter, 4, 1)
    ]);

    var clayPos   = await GetPositionPublicIdByNameAsync(_part1PublicId, "Clay");
    var ryanPos   = await GetPositionPublicIdByNameAsync(_part1PublicId, "Ryan");
    var darrenPos = await GetPositionPublicIdByNameAsync(_part1PublicId, "Darren");
    var philPos   = await GetPositionPublicIdByNameAsync(_part1PublicId, "Phil");

    await AssignParticipantToPositionAsync(_part1PublicId, clayPos,   _clayDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part1PublicId, ryanPos,   _ryanDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part1PublicId, darrenPos, _darrenDrafterPublicId, ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part1PublicId, philPos,   _philDrafterPublicId,   ParticipantKind.Drafter);
  }

  private async Task SetupPart2Async()
  {
    // Same 4 drafters + community; Bryan not yet in
    await AddDrafterParticipantAsync(_part2PublicId, _clayDrafterPublicId);
    await AddDrafterParticipantAsync(_part2PublicId, _ryanDrafterPublicId);
    await AddDrafterParticipantAsync(_part2PublicId, _darrenDrafterPublicId);
    await AddDrafterParticipantAsync(_part2PublicId, _philDrafterPublicId);
    await AddCommunityParticipantAsync(_part2PublicId);

    await AddPrimaryHostAsync(_part2PublicId, Shared.ClayHostPublicId);
    await AddCoHostAsync(_part2PublicId, Shared.RyanHostPublicId);

    // Positions: 4 drafters with 2-3 picks each for 10 total
    // Clay: 1,5,9 | Ryan: 2,6,10 | Darren: 3,7 | Phil: 4,8
    await SetPositionsAsync(_part2PublicId,
    [
      new DraftPositionRequest { Name = "Clay",   Picks = [1, 5, 9] },
      new DraftPositionRequest { Name = "Ryan",   Picks = [2, 6, 10] },
      new DraftPositionRequest { Name = "Darren", Picks = [3, 7] },
      new DraftPositionRequest { Name = "Phil",   Picks = [4, 8] }
    ]);

    await StartDraftPartAsync(_draftPublicId, 2);

    await AssignTriviaAsync(_part2PublicId,
    [
      (_ryanDrafterPublicId,   ParticipantKind.Drafter, 1, 4),
      (_clayDrafterPublicId,   ParticipantKind.Drafter, 2, 3),
      (_philDrafterPublicId,   ParticipantKind.Drafter, 3, 2),
      (_darrenDrafterPublicId, ParticipantKind.Drafter, 4, 1)
    ]);

    var clayPos   = await GetPositionPublicIdByNameAsync(_part2PublicId, "Clay");
    var ryanPos   = await GetPositionPublicIdByNameAsync(_part2PublicId, "Ryan");
    var darrenPos = await GetPositionPublicIdByNameAsync(_part2PublicId, "Darren");
    var philPos   = await GetPositionPublicIdByNameAsync(_part2PublicId, "Phil");

    await AssignParticipantToPositionAsync(_part2PublicId, clayPos,   _clayDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part2PublicId, ryanPos,   _ryanDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part2PublicId, darrenPos, _darrenDrafterPublicId, ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part2PublicId, philPos,   _philDrafterPublicId,   ParticipantKind.Drafter);
  }

  private async Task SetupPart3Async()
  {
    // 5 drafters (Bryan joins) + community
    await AddDrafterParticipantAsync(_part3PublicId, _clayDrafterPublicId);
    await AddDrafterParticipantAsync(_part3PublicId, _ryanDrafterPublicId);
    await AddDrafterParticipantAsync(_part3PublicId, _darrenDrafterPublicId);
    await AddDrafterParticipantAsync(_part3PublicId, _philDrafterPublicId);
    await AddDrafterParticipantAsync(_part3PublicId, _bryanDrafterPublicId);
    await AddCommunityParticipantAsync(_part3PublicId);

    // Bryan is both host and drafter in Part 3 (dual-role)
    await AddPrimaryHostAsync(_part3PublicId, Shared.BryanHostPublicId);
    await AddCoHostAsync(_part3PublicId, Shared.ClayHostPublicId);

    // Positions: 5 drafters — Clay: 1,6 | Ryan: 2,7 | Darren: 3,8 | Phil: 4 | Bryan: 5
    await SetPositionsAsync(_part3PublicId,
    [
      new DraftPositionRequest { Name = "Clay",   Picks = [1, 6] },
      new DraftPositionRequest { Name = "Ryan",   Picks = [2, 7] },
      new DraftPositionRequest { Name = "Darren", Picks = [3, 8] },
      new DraftPositionRequest { Name = "Phil",   Picks = [4] },
      new DraftPositionRequest { Name = "Bryan",  Picks = [5] }
    ]);

    await StartDraftPartAsync(_draftPublicId, 3);

    await AssignTriviaAsync(_part3PublicId,
    [
      (_darrenDrafterPublicId, ParticipantKind.Drafter, 1, 3),
      (_clayDrafterPublicId,   ParticipantKind.Drafter, 2, 2),
      (_ryanDrafterPublicId,   ParticipantKind.Drafter, 3, 2),
      (_philDrafterPublicId,   ParticipantKind.Drafter, 4, 1),
      (_bryanDrafterPublicId,  ParticipantKind.Drafter, 5, 1)
    ]);

    var clayPos   = await GetPositionPublicIdByNameAsync(_part3PublicId, "Clay");
    var ryanPos   = await GetPositionPublicIdByNameAsync(_part3PublicId, "Ryan");
    var darrenPos = await GetPositionPublicIdByNameAsync(_part3PublicId, "Darren");
    var philPos   = await GetPositionPublicIdByNameAsync(_part3PublicId, "Phil");
    var bryanPos  = await GetPositionPublicIdByNameAsync(_part3PublicId, "Bryan");

    await AssignParticipantToPositionAsync(_part3PublicId, clayPos,   _clayDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part3PublicId, ryanPos,   _ryanDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part3PublicId, darrenPos, _darrenDrafterPublicId, ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part3PublicId, philPos,   _philDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_part3PublicId, bryanPos,  _bryanDrafterPublicId,  ParticipantKind.Drafter);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Pick sequences
  // ─────────────────────────────────────────────────────────────────────────

  private async Task PlayPart1PicksAsync()
  {
    // 12 picks; play in descending position order
    // Clay: 9,5,1 | Ryan: 10,6,2 | Darren: 11,7,3 | Phil: 12,8,4
    await PlayPickAsync(_part1PublicId, 12, 12, _philDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[11]);
    await PlayPickAsync(_part1PublicId, 11, 11, _darrenDrafterPublicId, ParticipantKind.Drafter, _part1MoviePublicIds[10]);
    await PlayPickAsync(_part1PublicId, 10, 10, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[9]);
    await PlayPickAsync(_part1PublicId,  9,  9, _clayDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[8]);
    await PlayPickAsync(_part1PublicId,  8,  8, _philDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[7]);
    await PlayPickAsync(_part1PublicId,  7,  7, _darrenDrafterPublicId, ParticipantKind.Drafter, _part1MoviePublicIds[6]);
    await PlayPickAsync(_part1PublicId,  6,  6, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[5]);
    await PlayPickAsync(_part1PublicId,  5,  5, _clayDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[4]);
    await PlayPickAsync(_part1PublicId,  4,  4, _philDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[3]);
    await PlayPickAsync(_part1PublicId,  3,  3, _darrenDrafterPublicId, ParticipantKind.Drafter, _part1MoviePublicIds[2]);
    await PlayPickAsync(_part1PublicId,  2,  2, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[1]);
    await PlayPickAsync(_part1PublicId,  1,  1, _clayDrafterPublicId,   ParticipantKind.Drafter, _part1MoviePublicIds[0]);
  }

  private async Task PlayPart2PicksAsync()
  {
    // 10 picks; Clay 3, Ryan 3, Darren 2, Phil 2
    await PlayPickAsync(_part2PublicId, 10, 10, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[9]);
    await PlayPickAsync(_part2PublicId,  9,  9, _clayDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[8]);
    await PlayPickAsync(_part2PublicId,  8,  8, _philDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[7]);
    await PlayPickAsync(_part2PublicId,  7,  7, _darrenDrafterPublicId, ParticipantKind.Drafter, _part2MoviePublicIds[6]);
    await PlayPickAsync(_part2PublicId,  6,  6, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[5]);
    await PlayPickAsync(_part2PublicId,  5,  5, _clayDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[4]);
    await PlayPickAsync(_part2PublicId,  4,  4, _philDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[3]);
    await PlayPickAsync(_part2PublicId,  3,  3, _darrenDrafterPublicId, ParticipantKind.Drafter, _part2MoviePublicIds[2]);
    await PlayPickAsync(_part2PublicId,  2,  2, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[1]);
    await PlayPickAsync(_part2PublicId,  1,  1, _clayDrafterPublicId,   ParticipantKind.Drafter, _part2MoviePublicIds[0]);
  }

  private async Task PlayPart3PicksAsync()
  {
    // 8 picks; Clay: 6,1 | Ryan: 7,2 | Darren: 8,3 | Phil: 4 | Bryan: 5
    await PlayPickAsync(_part3PublicId, 8, 8, _darrenDrafterPublicId, ParticipantKind.Drafter, _part3MoviePublicIds[7]);
    await PlayPickAsync(_part3PublicId, 7, 7, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part3MoviePublicIds[6]);
    await PlayPickAsync(_part3PublicId, 6, 6, _clayDrafterPublicId,   ParticipantKind.Drafter, _part3MoviePublicIds[5]);
    await PlayPickAsync(_part3PublicId, 5, 5, _bryanDrafterPublicId,  ParticipantKind.Drafter, _part3MoviePublicIds[4]);
    await PlayPickAsync(_part3PublicId, 4, 4, _philDrafterPublicId,   ParticipantKind.Drafter, _part3MoviePublicIds[3]);
    await PlayPickAsync(_part3PublicId, 3, 3, _darrenDrafterPublicId, ParticipantKind.Drafter, _part3MoviePublicIds[2]);
    await PlayPickAsync(_part3PublicId, 2, 2, _ryanDrafterPublicId,   ParticipantKind.Drafter, _part3MoviePublicIds[1]);
    await PlayPickAsync(_part3PublicId, 1, 1, _clayDrafterPublicId,   ParticipantKind.Drafter, _part3MoviePublicIds[0]);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────────────────────────────────

  private async Task<string[]> GetMoviePublicIdsAsync(string[] imdbIds)
  {
    var result = new string[imdbIds.Length];
    for (var i = 0; i < imdbIds.Length; i++)
    {
      result[i] = await GetMoviePublicIdByImdbIdAsync(imdbIds[i]);
    }

    return result;
  }
}
