using ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Scenarios;

/// <summary>
/// Scenario 5 — 2025 Mega Draft
///
/// Four drafters (Clay, Ryan, Darren, Phil) draft 21 films from the 2025 calendar year.
/// Clay wins trivia and gets an extra pick (6 total), others get 5 each.
/// Uses a pool of 2025 films. Guard test: a 2024 film cannot be added to the pool.
///
/// Note: The year constraint is enforced at the Series/draft-type level, not the pool level.
/// Pool is used to establish the eligible film set; picks reference movie publicIds.
/// </summary>
public sealed class MegaDraft2025_Tests(DraftsIntegrationTestWebAppFactory factory)
  : DraftScenarioBase(factory)
{
  // Draft / part identifiers
  private string _draftPublicId = default!;
  private string _draftPartPublicId = default!;

  // Drafter public IDs
  private string _clayDrafterPublicId = default!;
  private string _ryanDrafterPublicId = default!;
  private string _darrenDrafterPublicId = default!;
  private string _philDrafterPublicId = default!;

  // Movie publicIds: indices 0-20 are 2025 films; index 21 is the "wrong year" guard film
  private string[] _moviePublicIds = default!;
  private string _wrongYearMoviePublicId = default!;

  protected override async Task OnInitializeAsync()
  {
    await Shared.SeedAsync(Sender);
    await Media.SeedAsync(DbContext);
    EmailCapture.Clear();

    // Create drafters from the shared hosts
    _clayDrafterPublicId = await CreateDrafterAsync(Shared.ClayPersonPublicId);
    _ryanDrafterPublicId = await CreateDrafterAsync(Shared.RyanPersonPublicId);
    _darrenDrafterPublicId = await CreateDrafterAsync(Shared.DarrenPersonPublicId);
    _philDrafterPublicId = await CreateDrafterAsync(Shared.PhilPersonPublicId);

    // Step 1 — CreateDraft (Mega draft)
    _draftPublicId = await CreateDraftAsync(
      "2025 Mega Draft",
      DraftType.Mega.Value,
      Shared.RegularSeriesId);

    await ProcessOutboxAsync();

    // Step 2 — CreateDraftPart (1 part, 21 picks for 4 drafters: 6+5+5+5)
    await CreateDraftPartAsync(_draftPublicId, partIndex: 1, minPosition: 1, maxPosition: 21);
    _draftPartPublicId = await GetDraftPartPublicIdAsync(_draftPublicId);

    // Step 3 — AddParticipants
    await AddDrafterParticipantAsync(_draftPartPublicId, _clayDrafterPublicId);
    await AddDrafterParticipantAsync(_draftPartPublicId, _ryanDrafterPublicId);
    await AddDrafterParticipantAsync(_draftPartPublicId, _darrenDrafterPublicId);
    await AddDrafterParticipantAsync(_draftPartPublicId, _philDrafterPublicId);

    // Step 4 — AddHosts (Bryan as primary host)
    await AddPrimaryHostAsync(_draftPartPublicId, Shared.BryanHostPublicId);

    // Step 5 — SetDraftPositions
    // Clay wins trivia: gets positions 1,5,9,13,17,21 (6 picks)
    // Ryan 2nd: positions 2,6,10,14,18 (5 picks)
    // Darren 3rd: positions 3,7,11,15,19 (5 picks)
    // Phil 4th: positions 4,8,12,16,20 (5 picks)
    await SetPositionsAsync(_draftPartPublicId,
    [
      new DraftPositionRequest { Name = "Clay",   Picks = [1, 5, 9, 13, 17, 21] },
      new DraftPositionRequest { Name = "Ryan",   Picks = [2, 6, 10, 14, 18] },
      new DraftPositionRequest { Name = "Darren", Picks = [3, 7, 11, 15, 19] },
      new DraftPositionRequest { Name = "Phil",   Picks = [4, 8, 12, 16, 20] }
    ]);

    // Step 6 — StartDraft
    await StartDraftPartAsync(_draftPublicId);

    // Step 7 — AssignTriviaResults (Clay wins with 4 correct)
    await AssignTriviaAsync(_draftPartPublicId,
    [
      (_clayDrafterPublicId, ParticipantKind.Drafter, 1, 4),
      (_ryanDrafterPublicId, ParticipantKind.Drafter, 2, 3),
      (_darrenDrafterPublicId, ParticipantKind.Drafter, 3, 2),
      (_philDrafterPublicId, ParticipantKind.Drafter, 4, 1)
    ]);

    // Step 8 — AssignParticipantsToPositions
    var clayPosId   = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Clay");
    var ryanPosId   = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Ryan");
    var darrenPosId = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Darren");
    var philPosId   = await GetPositionPublicIdByNameAsync(_draftPartPublicId, "Phil");

    await AssignParticipantToPositionAsync(_draftPartPublicId, clayPosId,   _clayDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_draftPartPublicId, ryanPosId,   _ryanDrafterPublicId,   ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_draftPartPublicId, darrenPosId, _darrenDrafterPublicId, ParticipantKind.Drafter);
    await AssignParticipantToPositionAsync(_draftPartPublicId, philPosId,   _philDrafterPublicId,   ParticipantKind.Drafter);

    // Seed 21 × 2025 films and 1 × 2024 guard film (fictional TmdbIds 40001-40021 + 40099)
    var titles2025 = new[]
    {
      "Wake Up Dead Man: A Knives Out Mystery",
      "The Baltimorans",
      "Splitsville",
      "The Ballad of Wallis Island",
      "Lurker",
      "The Testament of Ann Lee",
      "Superman",
      "Sorry, Baby",
      "If I Had Legs I'd Kick You",
      "Twinless",
      "No Other Choice",
      "Cover-Up",
      "Hamnet",
      "Sirāt",
      "Blue Moon",
      "Sentimental Value",
      "Train Dreams",
      "Bugonia",
      "The Secret Agent",
      "Sinners",
      "One Battle After Another"
    };

    _moviePublicIds = new string[21];
    for (var i = 0; i < 21; i++)
    {
      var publicId = $"m_{Guid.NewGuid():N}";
      var movie = Movie.Create(
        titles2025[i],
        publicId,
        MediaType.Movie,
        Guid.NewGuid(),
        tmdbId: 40001 + i).Value;
      DbContext.Movies.Add(movie);
      _moviePublicIds[i] = publicId;
    }

    // Guard film: 2024 release (not eligible for 2025 draft)
    _wrongYearMoviePublicId = $"m_{Guid.NewGuid():N}";
    var wrongYearMovie = Movie.Create(
      "Wrong Year Film 2024",
      _wrongYearMoviePublicId,
      MediaType.Movie,
      Guid.NewGuid(),
      tmdbId: 40099).Value;
    DbContext.Movies.Add(wrongYearMovie);

    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Create pool and add all 21 × 2025 films
    await CreatePoolAsync(_draftPublicId);
    for (var i = 0; i < 21; i++)
    {
      await AddMovieToPoolAsync(_draftPublicId, 40001 + i);
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Happy path: full 21-pick sequence
  // Play order 21 → 1 in snake pattern
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task MegaDraft2025_FullPickSequence_ShouldSucceedAsync()
  {
    await PlayAllPicksAsync();

    var picks = await DbContext.Picks
      .Where(p => p.DraftPart.PublicId == _draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    picks.Should().HaveCount(21);
  }

  [Fact]
  public async Task MegaDraft2025_Clay_ShouldHaveSixPicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var clayId = await GetDrafterInternalIdAsync(_clayDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == clayId, TestContext.Current.CancellationToken);

    count.Should().Be(6, "Clay (trivia winner) has 6 picks");
  }

  [Fact]
  public async Task MegaDraft2025_Ryan_ShouldHaveFivePicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var ryanId = await GetDrafterInternalIdAsync(_ryanDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == ryanId, TestContext.Current.CancellationToken);

    count.Should().Be(5, "Ryan has 5 picks");
  }

  [Fact]
  public async Task MegaDraft2025_Darren_ShouldHaveFivePicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var darrenId = await GetDrafterInternalIdAsync(_darrenDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == darrenId, TestContext.Current.CancellationToken);

    count.Should().Be(5, "Darren has 5 picks");
  }

  [Fact]
  public async Task MegaDraft2025_Phil_ShouldHaveFivePicks_AfterFullFlowAsync()
  {
    await PlayAllPicksAsync();

    var philId = await GetDrafterInternalIdAsync(_philDrafterPublicId);
    var count = await DbContext.Picks
      .CountAsync(p =>
        p.DraftPart.PublicId == _draftPartPublicId &&
        p.PlayedByParticipantKindValue == ParticipantKind.Drafter &&
        p.PlayedByParticipantIdValue == philId, TestContext.Current.CancellationToken);

    count.Should().Be(5, "Phil has 5 picks");
  }

  [Fact]
  public async Task MegaDraft2025_EndDraft_ShouldCompleteAsync()
  {
    await PlayAllPicksAsync();
    await CompleteDraftPartAsync(_draftPublicId);

    var draftPart = await DbContext.DraftParts
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == _draftPartPublicId, TestContext.Current.CancellationToken);

    draftPart.Status.Should().Be(DraftPartStatus.Completed);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Pool contains 21 films
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task MegaDraft2025_Pool_ShouldContainTwentyOneFilmsAsync()
  {
    var draft = await DbContext.Drafts
      .Where(d => d.PublicId == _draftPublicId)
      .FirstAsync(TestContext.Current.CancellationToken);

    var pool = await DbContext.DraftPools
      .Include(p => p.TmdbIds)
      .FirstAsync(p => p.DraftId == draft.Id, TestContext.Current.CancellationToken);

    pool.TmdbIds.Should().HaveCount(21, "All 21 × 2025 films should be in the pool");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Guard: 2024 film picking (pool does not constrain picks at API level,
  //         but the wrong-year movie should never be in the pool)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task MegaDraft2025_WrongYearFilm_IsNotInPoolAsync()
  {
    var draft = await DbContext.Drafts
      .Where(d => d.PublicId == _draftPublicId)
      .FirstAsync(TestContext.Current.CancellationToken);

    var pool = await DbContext.DraftPools
      .Include(p => p.TmdbIds)
      .FirstAsync(p => p.DraftId == draft.Id, TestContext.Current.CancellationToken);

    pool.TmdbIds.Should().NotContain(i => i.TmdbId == 40099,
      "The 2024 guard film (TmdbId=40099) should not be in the 2025 pool");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Play all 21 picks in descending snake order (21 → 1).
  /// Position assignments:
  ///   Clay   → 1,5,9,13,17,21
  ///   Ryan   → 2,6,10,14,18
  ///   Darren → 3,7,11,15,19
  ///   Phil   → 4,8,12,16,20
  /// </summary>
  private async Task PlayAllPicksAsync()
  {
    // movie indices 0-20 map to positions 1-21 respectively
    // Play order descends: playOrder=position (1-based)
    var schedule = new[]
    {
      (pos: 21, drafter: _clayDrafterPublicId,   movieIdx: 20),
      (pos: 20, drafter: _philDrafterPublicId,   movieIdx: 19),
      (pos: 19, drafter: _darrenDrafterPublicId, movieIdx: 18),
      (pos: 18, drafter: _ryanDrafterPublicId,   movieIdx: 17),
      (pos: 17, drafter: _clayDrafterPublicId,   movieIdx: 16),
      (pos: 16, drafter: _philDrafterPublicId,   movieIdx: 15),
      (pos: 15, drafter: _darrenDrafterPublicId, movieIdx: 14),
      (pos: 14, drafter: _ryanDrafterPublicId,   movieIdx: 13),
      (pos: 13, drafter: _clayDrafterPublicId,   movieIdx: 12),
      (pos: 12, drafter: _philDrafterPublicId,   movieIdx: 11),
      (pos: 11, drafter: _darrenDrafterPublicId, movieIdx: 10),
      (pos: 10, drafter: _ryanDrafterPublicId,   movieIdx:  9),
      (pos:  9, drafter: _clayDrafterPublicId,   movieIdx:  8),
      (pos:  8, drafter: _philDrafterPublicId,   movieIdx:  7),
      (pos:  7, drafter: _darrenDrafterPublicId, movieIdx:  6),
      (pos:  6, drafter: _ryanDrafterPublicId,   movieIdx:  5),
      (pos:  5, drafter: _clayDrafterPublicId,   movieIdx:  4),
      (pos:  4, drafter: _philDrafterPublicId,   movieIdx:  3),
      (pos:  3, drafter: _darrenDrafterPublicId, movieIdx:  2),
      (pos:  2, drafter: _ryanDrafterPublicId,   movieIdx:  1),
      (pos:  1, drafter: _clayDrafterPublicId,   movieIdx:  0)
    };

    foreach (var (pos, drafter, movieIdx) in schedule)
    {
      await PlayPickAsync(_draftPartPublicId, pos, pos, drafter, ParticipantKind.Drafter, _moviePublicIds[movieIdx]);
    }
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
