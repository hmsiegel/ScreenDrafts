namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// Golden-output and behaviour tests for the page footer, which drafts are selected and in what
/// order, and the rule that Patreon-only material never appears in the export.
/// </summary>
public sealed class ExportDraftsWikiSelectionTests(DraftsIntegrationTestWebAppFactory factory)
  : WikiExportIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Footer
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldEndWithReferencesYearCategoryAndOneCategoryPerLiveAppCategoryAsync()
  {
    var draft = await Seed.DraftAsync("Footer Draft", episodeNumber: 1);

    // The year comes from the FIRST main-feed release: part I airs in 2023, part II in 2024.
    await Seed.PartAsync(draft, 1, mainFeed: Date(2023, 12, 25));
    await Seed.PartAsync(draft, 2, mainFeed: Date(2024, 1, 8));
    await Seed.CategoryAsync(draft, "Zeta");
    await Seed.CategoryAsync(draft, "Alpha");
    await Seed.CategoryAsync(draft, "Retired Category", isDeleted: true);

    var page = await ExportDraftPageAsync(draft);

    page
      .Should()
      .EndWith(
        "==References==\n"
          + "<references />\n"
          + "[[Category:2023 Drafts]]\n"
          + "[[Category:Alpha]]\n"
          + "[[Category:Zeta]]"
      );
    page.Should().NotContain("Retired Category");
  }

  [Fact]
  public async Task Export_ShouldEndWithJustTheYearCategory_WhenTheDraftHasNoAppCategoriesAsync()
  {
    var draft = await Seed.DraftAsync("Plain Footer", episodeNumber: 1);
    await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 6, 1));

    (await ExportDraftPageAsync(draft))
      .Should()
      .EndWith("==References==\n<references />\n[[Category:2024 Drafts]]");
  }

  // -------------------------------------------------------------------------
  // Selection and order
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldReturnPagesInTheRequestedOrder_AndExportDuplicateIdsOnceAsync()
  {
    var a = await Seed.DraftAsync("Alpha Draft", episodeNumber: 1);
    var b = await Seed.DraftAsync("Bravo Draft", episodeNumber: 2);
    var c = await Seed.DraftAsync("Charlie Draft", episodeNumber: 3);
    await Seed.PartAsync(a, 1, mainFeed: Date(2024, 1, 1));
    await Seed.PartAsync(b, 1, mainFeed: Date(2024, 2, 1));
    await Seed.PartAsync(c, 1, mainFeed: Date(2024, 3, 1));

    var result = await ExportDraftsAsync(c.PublicId, a.PublicId, b.PublicId, a.PublicId);

    result.IsSuccess.Should().BeTrue();
    PageTitles(result.Value).Should().Equal("Charlie Draft", "Alpha Draft", "Bravo Draft");
    result.Value.PageCount.Should().Be(3);
  }

  [Fact]
  public async Task Export_ShouldSilentlySkipUnknownIds_WhenAtLeastOneDraftIsFoundAsync()
  {
    var a = await Seed.DraftAsync("Alpha Draft", episodeNumber: 1);
    await Seed.PartAsync(a, 1, mainFeed: Date(2024, 1, 1));

    var result = await ExportDraftsAsync("d_doesnotexist000", a.PublicId);

    result.IsSuccess.Should().BeTrue();
    PageTitles(result.Value).Should().Equal("Alpha Draft");
    result.Value.PageCount.Should().Be(1);
  }

  [Fact]
  public async Task Export_ShouldReturnNoDraftsFound_WhenNoRequestedIdExistsAsync()
  {
    var result = await ExportDraftsAsync("d_doesnotexist000");

    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle().Which.Should().Be(WikiExportErrors.NoDraftsFound);
  }

  // -------------------------------------------------------------------------
  // No Patreon material
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldReturnOnlyMainFeedDrafts_AndFewerPagesThanRequested_WhenTheSelectionIsMixedAsync()
  {
    var main = await Seed.DraftAsync("Main Feed Draft", episodeNumber: 1);
    await Seed.PartAsync(main, 1, mainFeed: Date(2024, 1, 1));
    var patreon = await Seed.DraftAsync("Patreon Only Draft");
    await Seed.PartAsync(patreon, 1, patreon: Date(2024, 1, 2));

    var result = await ExportDraftsAsync(patreon.PublicId, main.PublicId);

    result.IsSuccess.Should().BeTrue();
    PageTitles(result.Value).Should().Equal("Main Feed Draft");
    result.Value.PageCount.Should().Be(1, "two were requested but one is Patreon-only");
    result.Value.Content.Should().NotContain("Patreon Only Draft");
  }

  [Fact]
  public async Task Export_ShouldReturnNoDraftsFound_WhenEveryRequestedDraftIsPatreonOnlyAsync()
  {
    var patreon = await Seed.DraftAsync("Patreon Only Draft");
    await Seed.PartAsync(patreon, 1, patreon: Date(2024, 1, 2));

    var result = await ExportDraftsAsync(patreon.PublicId);

    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle().Which.Should().Be(WikiExportErrors.NoDraftsFound);
  }

  [Fact]
  public async Task Export_ShouldDropPartsWithoutAMainFeedRelease_FromADraftThatHasSomeAsync()
  {
    var draft = await Seed.DraftAsync("Half Public", episodeNumber: 1);
    var p1 = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var p2 = await Seed.PartAsync(draft, 2, patreon: Date(2024, 1, 8));
    var alice = await Seed.DrafterAsync("Alice");
    var a1 = await Seed.ParticipantAsync(p1, alice);
    var a2 = await Seed.ParticipantAsync(p2, alice);
    await Seed.PickAsync(p1, a1, await Seed.MovieAsync("Public Film"), 7, 1);
    await Seed.PickAsync(p2, a2, await Seed.MovieAsync("Patreon Film"), 7, 1);

    var page = await ExportDraftPageAsync(draft);

    page.Should().Contain("[[Public Film]]");
    page.Should().NotContain("Patreon Film");
    TemplateBlock(page).Should().Contain(P("airDate", "January 1, 2024") + "\n");
    page.Should().NotContain("==Part ", "only one part is left, so no part headings are needed");
  }

  [Fact]
  public async Task Export_ShouldNeverUseAPatreonDraftAsAPreviousNextOrCampaignNeighbourAsync()
  {
    var campaign = await Seed.CampaignAsync("Mixed Campaign");
    var first = await Seed.DraftAsync("Public First", campaign, episodeNumber: 1);
    var secret = await Seed.DraftAsync("Patreon Middle", campaign);
    var last = await Seed.DraftAsync("Public Last", campaign, episodeNumber: 2);
    await Seed.PartAsync(first, 1, mainFeed: Date(2024, 1, 1));
    await Seed.PartAsync(secret, 1, patreon: Date(2024, 2, 1));
    await Seed.PartAsync(last, 1, mainFeed: Date(2024, 3, 1));

    var firstBlock = TemplateBlock(await ExportDraftPageAsync(first));
    var lastBlock = TemplateBlock(await ExportDraftPageAsync(last));

    firstBlock.Should().Contain(P("nextEpisode", "[[Public Last]]"));
    firstBlock.Should().Contain(P("NextInSeries", "[[Public Last]]"));
    lastBlock.Should().Contain(P("previousEpisode", "[[Public First]]"));
    lastBlock.Should().Contain(P("LastInSeries", "[[Public First]]"));
    firstBlock.Should().NotContain("Patreon Middle");
    lastBlock.Should().NotContain("Patreon Middle");
  }
}
