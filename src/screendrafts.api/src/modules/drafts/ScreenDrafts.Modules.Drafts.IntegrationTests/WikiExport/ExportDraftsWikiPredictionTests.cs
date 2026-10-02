namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// Golden-output tests for the <c>==Predictions==</c> section of an episode page.
/// </summary>
public sealed class ExportDraftsWikiPredictionTests(DraftsIntegrationTestWebAppFactory factory)
  : WikiExportIntegrationTest(factory)
{
  private const int UnorderedAll = 0;
  private const int UnorderedTopN = 1;
  private const int OrderedTopN = 2;
  private const int OrderedAll = 3;

  private static readonly DateTime _submitted = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

  private static string PredictionsBody(string page) =>
    Between(page, "==Predictions==\n\n", "\n\n==References==")["==Predictions==\n\n".Length..];

  private sealed record Setup(
    DraftSeed Draft,
    PartSeed Part,
    Guid Season,
    Guid Matt,
    Guid William
  );

  private async Task<Setup> NewSetupAsync(int? mode)
  {
    var draft = await Seed.DraftAsync("Prediction Draft", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var season = await Seed.PredictionSeasonAsync(1, Date(2023, 12, 1));
    var matt = await Seed.ContestantAsync("Matt Singer");
    var william = await Seed.ContestantAsync("William Hurt");

    if (mode is { } value)
    {
      await Seed.PredictionRuleAsync(part, value);
    }

    return new Setup(draft, part, season, matt, william);
  }

  /// <summary>
  /// Matt predicted Hit One (idx 5, hit), Miss One (idx 1, miss), Hit Two (idx 2, hit);
  /// William predicted Hit Three (idx 9, hit) and Miss Two (idx 3, miss). Matt has 12 points
  /// and William 7.
  /// </summary>
  private async Task SeedStandardSetsAsync(Setup s)
  {
    var mattSet = await Seed.PredictionSetAsync(s.Part, s.Matt, s.Season, _submitted);
    var williamSet = await Seed.PredictionSetAsync(
      s.Part,
      s.William,
      s.Season,
      _submitted.AddMinutes(1)
    );

    await Seed.PredictionEntryAsync(mattSet, "Hit One", isCorrect: true, orderIndex: 5);
    await Seed.PredictionEntryAsync(mattSet, "Miss One", isCorrect: false, orderIndex: 1);
    await Seed.PredictionEntryAsync(mattSet, "Hit Two", isCorrect: true, orderIndex: 2);
    await Seed.PredictionEntryAsync(williamSet, "Hit Three", isCorrect: true, orderIndex: 9);
    await Seed.PredictionEntryAsync(williamSet, "Miss Two", isCorrect: false, orderIndex: 3);

    await Seed.PredictionResultAsync(mattSet, 12);
    await Seed.PredictionResultAsync(williamSet, 7);
  }

  // -------------------------------------------------------------------------
  // Empty and unrevealed
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldWriteATodoComment_WhenNoPredictionSetsWereRecordedAsync()
  {
    var s = await NewSetupAsync(mode: null);

    var page = await ExportDraftPageAsync(s.Draft);

    page.Should().Contain("==Predictions==\n\n");
    PredictionsBody(page).Should().Be("<!-- TODO: no predictions recorded for this draft -->");
  }

  [Fact]
  public async Task Export_ShouldSayPredictionsWereNotRevealed_WhenNoEntryHasBeenScoredAsync()
  {
    var s = await NewSetupAsync(UnorderedAll);
    var set = await Seed.PredictionSetAsync(s.Part, s.Matt, s.Season, _submitted);
    await Seed.PredictionEntryAsync(set, "Unscored One", isCorrect: null);
    await Seed.PredictionEntryAsync(set, "Unscored Two", isCorrect: null);

    PredictionsBody(await ExportDraftPageAsync(s.Draft))
      .Should()
      .Be("Predictions were made but not revealed.");
  }

  [Fact]
  public async Task Export_ShouldSayPredictionsWereNotRevealed_WhenTheSetsHaveNoEntriesAsync()
  {
    var s = await NewSetupAsync(UnorderedAll);
    await Seed.PredictionSetAsync(s.Part, s.Matt, s.Season, _submitted);

    PredictionsBody(await ExportDraftPageAsync(s.Draft))
      .Should()
      .Be("Predictions were made but not revealed.");
  }

  // -------------------------------------------------------------------------
  // Unordered modes
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(UnorderedAll)]
  [InlineData(UnorderedTopN)]
  public async Task Export_ShouldRenderAHitsFirstTableWithPlainTitles_ForUnorderedModesAsync(int mode)
  {
    var s = await NewSetupAsync(mode);
    await SeedStandardSetsAsync(s);

    // order_index is populated on purpose: unordered modes must not number the cells.
    PredictionsBody(await ExportDraftPageAsync(s.Draft))
      .Should()
      .Be(
        string.Join(
          "\n",
          "{| class=\"fandom-table\"",
          "|+Predictions",
          "!Matt",
          "!William",
          "|-",
          "| bgcolor=\"lightgreen\" |[[Hit Two]]",
          "| bgcolor=\"lightgreen\" |[[Hit Three]]",
          "|-",
          "| bgcolor=\"lightgreen\" |[[Hit One]]",
          "| bgcolor=\"gray\" |[[Miss Two]]",
          "|-",
          "| bgcolor=\"gray\" |[[Miss One]]",
          "| bgcolor=\"gray\" |",
          "|-",
          "| colspan=\"2\" |'''CURRENT STANDINGS'''",
          "|-",
          "|'''Matt'''",
          "|'''William'''",
          "|-",
          "|12",
          "|7",
          "|}"
        )
      );
  }

  // -------------------------------------------------------------------------
  // Ordered modes
  // -------------------------------------------------------------------------

  // The wiki's real convention for order_index (is 1 the best slot or the worst? 0- or
  // 1-based?) is unverified. These tests pin only what the exporter does today: print
  // order_index exactly as stored, highest first, hits and misses alike.
  [Theory]
  [InlineData(OrderedTopN)]
  [InlineData(OrderedAll)]
  public async Task Export_ShouldShowOrderIndexAsStoredHighestFirst_ForOrderedModes_DirectionAndBaseUnverifiedAsync(
    int mode
  )
  {
    var s = await NewSetupAsync(mode);
    await SeedStandardSetsAsync(s);

    PredictionsBody(await ExportDraftPageAsync(s.Draft))
      .Should()
      .Be(
        string.Join(
          "\n",
          "{| class=\"fandom-table\"",
          "|+Predictions",
          "!Matt",
          "!William",
          "|-",
          "| bgcolor=\"lightgreen\" |5. [[Hit One]]",
          "| bgcolor=\"lightgreen\" |9. [[Hit Three]]",
          "|-",
          "| bgcolor=\"lightgreen\" |2. [[Hit Two]]",
          "| bgcolor=\"gray\" |3. [[Miss Two]]",
          "|-",
          "| bgcolor=\"gray\" |1. [[Miss One]]",
          "| bgcolor=\"gray\" |",
          "|-",
          "| colspan=\"2\" |'''CURRENT STANDINGS'''",
          "|-",
          "|'''Matt'''",
          "|'''William'''",
          "|-",
          "|12",
          "|7",
          "|}"
        )
      );
  }

  [Fact]
  public async Task Export_ShouldInferOrderedMode_WhenThereIsNoRulesRowAndEntriesHaveOrderIndexAsync()
  {
    var s = await NewSetupAsync(mode: null);
    var set = await Seed.PredictionSetAsync(s.Part, s.Matt, s.Season, _submitted);
    await Seed.PredictionEntryAsync(set, "Alpha", isCorrect: true, orderIndex: 2);
    await Seed.PredictionEntryAsync(set, "Beta", isCorrect: false, orderIndex: 1);

    var body = PredictionsBody(await ExportDraftPageAsync(s.Draft));

    body.Should().Contain("| bgcolor=\"lightgreen\" |2. [[Alpha]]\n");
    body.Should().Contain("| bgcolor=\"gray\" |1. [[Beta]]\n");
  }

  [Fact]
  public async Task Export_ShouldInferUnorderedMode_WhenThereIsNoRulesRowAndNoEntryHasAnOrderIndexAsync()
  {
    var s = await NewSetupAsync(mode: null);
    var set = await Seed.PredictionSetAsync(s.Part, s.Matt, s.Season, _submitted);
    await Seed.PredictionEntryAsync(set, "Alpha", isCorrect: true);
    await Seed.PredictionEntryAsync(set, "Beta", isCorrect: false);

    var body = PredictionsBody(await ExportDraftPageAsync(s.Draft));

    body.Should().Contain("| bgcolor=\"lightgreen\" |[[Alpha]]\n");
    body.Should().Contain("| bgcolor=\"gray\" |[[Beta]]\n");
  }

  // -------------------------------------------------------------------------
  // Standings and multi-part
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldCountOnlyResultsUpToThisPartsAirDatePlusAllCarryovers_PerPartSectionAsync()
  {
    var season = await Seed.PredictionSeasonAsync(1, Date(2023, 12, 1));
    var matt = await Seed.ContestantAsync("Matt Singer");

    // The exported draft has parts I (Jan 1) and II (Feb 1) with predictions, plus an
    // unpredicted part III (Feb 8). A later, separate draft airs on Mar 1.
    var draft = await Seed.DraftAsync("Standings Draft", episodeNumber: 1);
    var p1 = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var p2 = await Seed.PartAsync(draft, 2, mainFeed: Date(2024, 2, 1));
    await Seed.PartAsync(draft, 3, mainFeed: Date(2024, 2, 8));

    var later = await Seed.DraftAsync("Later Draft", episodeNumber: 2);
    var laterPart = await Seed.PartAsync(later, 1, mainFeed: Date(2024, 3, 1));

    var set1 = await Seed.PredictionSetAsync(p1, matt, season, _submitted);
    var set2 = await Seed.PredictionSetAsync(p2, matt, season, _submitted.AddDays(31));
    var setLater = await Seed.PredictionSetAsync(laterPart, matt, season, _submitted.AddDays(60));

    await Seed.PredictionEntryAsync(set1, "Part One Pick", isCorrect: true);
    await Seed.PredictionEntryAsync(set2, "Part Two Pick", isCorrect: true);
    await Seed.PredictionResultAsync(set1, 10);
    await Seed.PredictionResultAsync(set2, 5);
    await Seed.PredictionResultAsync(setLater, 7);
    await Seed.CarryoverAsync(matt, season, 3);

    var body = PredictionsBody(await ExportDraftPageAsync(draft));

    var partOne = Between(body, "===Part I===", "===Part II===");
    var partTwo = body[body.IndexOf("===Part II===", StringComparison.Ordinal)..];

    partOne.Should().Contain("[[Part One Pick]]").And.EndWith("|13\n|}\n\n");
    partTwo.Should().Contain("[[Part Two Pick]]").And.EndWith("|18\n|}");
    body.Should().NotContain("===Part III===", "parts without prediction sets get no sub-section");

    // If the later draft's 7 points leaked in, part I would read 20 and part II would read 25.
    body.Should().NotContain("|20\n").And.NotContain("|25\n");
  }

  [Fact]
  public async Task Export_ShouldNotUseASubSection_WhenOnlyOnePartExistsAsync()
  {
    var s = await NewSetupAsync(UnorderedAll);
    await SeedStandardSetsAsync(s);

    PredictionsBody(await ExportDraftPageAsync(s.Draft)).Should().StartWith("{| class=");
  }
}
