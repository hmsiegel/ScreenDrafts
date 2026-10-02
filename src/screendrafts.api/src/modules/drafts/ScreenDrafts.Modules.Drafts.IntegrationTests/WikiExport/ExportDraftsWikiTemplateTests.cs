namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// Golden-output tests for the <c>{{Episodes}}</c> template block, the series and neighbour
/// links, hosts, trivia, and the intro/structure of an episode page.
/// </summary>
public sealed class ExportDraftsWikiTemplateTests(DraftsIntegrationTestWebAppFactory factory)
  : WikiExportIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Template block
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldRenderTheTemplateBlockInTheDocumentedOrderAsync()
  {
    var draft = await Seed.DraftAsync("Draft Alpha", episodeNumber: 390);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 11, 1));
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var bob = await Seed.DrafterAsync("Bob Brown");
    var aliceRow = await Seed.ParticipantAsync(part, alice);
    await Seed.ParticipantAsync(part, bob);
    var film = await Seed.MovieAsync("Film A");
    await Seed.PickAsync(part, aliceRow, film, position: 7, playOrder: 1);
    var host = await Seed.HostAsync("Clay Host");
    await Seed.HostOnPartAsync(part, host, role: 0);
    var cohost = await Seed.HostAsync("Dana Cohost");
    await Seed.HostOnPartAsync(part, cohost, role: 1);

    var page = await ExportDraftPageAsync(draft);

    TemplateBlock(page)
      .Should()
      .Be(
        string.Join(
          "\n",
          "{{Episodes",
          P("title"),
          P("episodeNumber", "390"),
          P("image"),
          P("airDate", "November 1, 2024"),
          P("drafters", "[[Alice Anderson]]<br>[[Bob Brown]]"),
          P("trivia"),
          P("commish", "[[Clay Host]]"),
          P("cocommish", "[[Dana Cohost]]"),
          P("length"),
          P("previousEpisode"),
          P("nextEpisode"),
          "}}"
        )
      );
  }

  [Fact]
  public async Task Export_ShouldLeaveTitleImageAndLengthBlank_AndFormatEachAirDateAsMonthDayYearAsync()
  {
    var draft = await Seed.DraftAsync("Two Parter", episodeNumber: 12);
    await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 3, 5));
    await Seed.PartAsync(draft, 2, mainFeed: Date(2024, 3, 12));

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    block.Should().Contain(P("title") + "\n");
    block.Should().Contain(P("image") + "\n");
    block.Should().Contain(P("length") + "\n");
    block.Should().Contain(P("airDate", "March 5, 2024<br>March 12, 2024"));
  }

  [Fact]
  public async Task Export_ShouldOrderDraftersByFirstPickThenName_AndExcludeCommunityParticipantsAsync()
  {
    var draft = await Seed.DraftAsync("Ordering Draft", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var zed = await Seed.DrafterAsync("Zed Zimmer");
    var amy = await Seed.DrafterAsync("Amy Adams");
    var bea = await Seed.DrafterAsync("Bea Bell");
    var cal = await Seed.DrafterAsync("Cal Cole");

    // Seeded in "wrong" order on purpose: the page must follow first-pick order.
    var zedRow = await Seed.ParticipantAsync(part, zed);
    var amyRow = await Seed.ParticipantAsync(part, amy);
    await Seed.ParticipantAsync(part, bea);
    await Seed.ParticipantAsync(part, cal);
    await Seed.CommunityAsync(part);

    await Seed.PickAsync(part, zedRow, await Seed.MovieAsync("F1"), position: 7, playOrder: 1);
    await Seed.PickAsync(part, amyRow, await Seed.MovieAsync("F2"), position: 6, playOrder: 2);

    // Bea and Cal never pick, so they sort after the pickers, and then by name.

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    block
      .Should()
      .Contain(P("drafters", "[[Zed Zimmer]]<br>[[Amy Adams]]<br>[[Bea Bell]]<br>[[Cal Cole]]"));
    block.Should().NotContain("Patreon Members");
  }

  // -------------------------------------------------------------------------
  // Series (the draft's campaign)
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldRenderSeriesLinesByMainFeedAirDate_WhenTheDraftIsInACampaignAsync()
  {
    var campaign = await Seed.CampaignAsync("Summer Series");
    var first = await Seed.DraftAsync("Series First", campaign, episodeNumber: 1);
    var middle = await Seed.DraftAsync("Series Middle", campaign, episodeNumber: 2);
    var last = await Seed.DraftAsync("Series Last", campaign, episodeNumber: 3);

    // Created in a scrambled order; air dates decide the sequence.
    await Seed.PartAsync(last, 1, mainFeed: Date(2024, 3, 1));
    await Seed.PartAsync(first, 1, mainFeed: Date(2024, 1, 1));
    await Seed.PartAsync(middle, 1, mainFeed: Date(2024, 2, 1));

    var middleBlock = TemplateBlock(await ExportDraftPageAsync(middle));
    var firstBlock = TemplateBlock(await ExportDraftPageAsync(first));
    var lastBlock = TemplateBlock(await ExportDraftPageAsync(last));

    middleBlock.Should().Contain(P("NameOfSeries", "'''Summer Series'''"));
    middleBlock.Should().Contain(P("LastInSeries", "[[Series First]]"));
    middleBlock.Should().Contain(P("NextInSeries", "[[Series Last]]"));

    firstBlock.Should().Contain(P("LastInSeries") + "\n");
    firstBlock.Should().Contain(P("NextInSeries", "[[Series Middle]]"));

    lastBlock.Should().Contain(P("LastInSeries", "[[Series Middle]]"));
    lastBlock.Should().Contain(P("NextInSeries") + "\n");
  }

  [Fact]
  public async Task Export_ShouldOmitAllThreeSeriesLines_WhenTheDraftHasNoCampaignAsync()
  {
    var draft = await Seed.DraftAsync("Standalone", episodeNumber: 5);
    await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    block.Should().NotContain("NameOfSeries");
    block.Should().NotContain("LastInSeries");
    block.Should().NotContain("NextInSeries");
  }

  // -------------------------------------------------------------------------
  // Previous / next episode
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldLinkPreviousAndNextEpisodeByMainFeedAirDateAcrossAllDraftsAsync()
  {
    var before = await Seed.DraftAsync("Before", episodeNumber: 1);
    var current = await Seed.DraftAsync("Current", episodeNumber: 2);
    var after = await Seed.DraftAsync("After", episodeNumber: 3);
    await Seed.PartAsync(after, 1, mainFeed: Date(2024, 3, 1));
    await Seed.PartAsync(before, 1, mainFeed: Date(2024, 1, 1));
    await Seed.PartAsync(current, 1, mainFeed: Date(2024, 2, 1));

    var block = TemplateBlock(await ExportDraftPageAsync(current));

    block.Should().Contain(P("previousEpisode", "[[Before]]"));
    block.Should().Contain(P("nextEpisode", "[[After]]"));
  }

  [Fact]
  public async Task Export_ShouldGiveAMultiPartDraftOneNeighbourEntryPerPart_JoinedByDoubleBreaksAsync()
  {
    var x = await Seed.DraftAsync("Xavier", episodeNumber: 1);
    var m = await Seed.DraftAsync("Multi", episodeNumber: 2);
    var q = await Seed.DraftAsync("Quincy", episodeNumber: 3);
    var z = await Seed.DraftAsync("Zelda", episodeNumber: 4);

    // Air order: Xavier, Multi I, Quincy, Multi II, Zelda
    await Seed.PartAsync(x, 1, mainFeed: Date(2024, 1, 1));
    await Seed.PartAsync(m, 1, mainFeed: Date(2024, 2, 1));
    await Seed.PartAsync(q, 1, mainFeed: Date(2024, 2, 15));
    await Seed.PartAsync(m, 2, mainFeed: Date(2024, 3, 1));
    await Seed.PartAsync(z, 1, mainFeed: Date(2024, 4, 1));

    var block = TemplateBlock(await ExportDraftPageAsync(m));

    block.Should().Contain(P("previousEpisode", "[[Xavier]]<br><br>[[Quincy]]"));
    block.Should().Contain(P("nextEpisode", "[[Quincy]]<br><br>[[Zelda]]"));
  }

  [Fact]
  public async Task Export_ShouldSkipNeighboursThatBelongToTheSameDraftAsync()
  {
    var x = await Seed.DraftAsync("Xavier", episodeNumber: 1);
    var m = await Seed.DraftAsync("Multi", episodeNumber: 2);
    var z = await Seed.DraftAsync("Zelda", episodeNumber: 3);

    // Multi's own parts are adjacent on the timeline; neither may be its own neighbour.
    await Seed.PartAsync(x, 1, mainFeed: Date(2024, 1, 1));
    await Seed.PartAsync(m, 1, mainFeed: Date(2024, 2, 1));
    await Seed.PartAsync(m, 2, mainFeed: Date(2024, 2, 8));
    await Seed.PartAsync(z, 1, mainFeed: Date(2024, 3, 1));

    var block = TemplateBlock(await ExportDraftPageAsync(m));

    block.Should().Contain(P("previousEpisode", "[[Xavier]]") + "\n");
    block.Should().Contain(P("nextEpisode", "[[Zelda]]") + "\n");
  }

  [Fact]
  public async Task Export_ShouldLabelAMultiPartNeighbourWithItsPartNumeralAsync()
  {
    var x = await Seed.DraftAsync("Xavier", episodeNumber: 1);
    var m = await Seed.DraftAsync("Multi", episodeNumber: 2);
    var z = await Seed.DraftAsync("Zelda", episodeNumber: 3);
    await Seed.PartAsync(x, 1, mainFeed: Date(2024, 1, 1));
    await Seed.PartAsync(m, 1, mainFeed: Date(2024, 2, 1));
    await Seed.PartAsync(m, 2, mainFeed: Date(2024, 2, 8));
    await Seed.PartAsync(z, 1, mainFeed: Date(2024, 3, 1));

    var xavierBlock = TemplateBlock(await ExportDraftPageAsync(x));
    var zeldaBlock = TemplateBlock(await ExportDraftPageAsync(z));

    xavierBlock.Should().Contain(P("nextEpisode", "[[Multi]] Part I"));
    zeldaBlock.Should().Contain(P("previousEpisode", "[[Multi]] Part II"));
  }

  // -------------------------------------------------------------------------
  // Hosts
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldMapRoleZeroToCommishAndOtherRolesToCocommishAsync()
  {
    var draft = await Seed.DraftAsync("Hosted", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    await Seed.HostOnPartAsync(part, await Seed.HostAsync("Pat Primary"), role: 0);
    await Seed.HostOnPartAsync(part, await Seed.HostAsync("Casey Cohost"), role: 1);

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    block.Should().Contain(P("commish", "[[Pat Primary]]"));
    block.Should().Contain(P("cocommish", "[[Casey Cohost]]"));
  }

  [Fact]
  public async Task Export_ShouldSuffixAHostWhoIsNotOnEveryPartAsync()
  {
    var draft = await Seed.DraftAsync("Three Parts", episodeNumber: 1);
    var p1 = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var p2 = await Seed.PartAsync(draft, 2, mainFeed: Date(2024, 1, 8));
    var p3 = await Seed.PartAsync(draft, 3, mainFeed: Date(2024, 1, 15));

    var primary = await Seed.HostAsync("Pat Primary");
    var early = await Seed.HostAsync("Early Cohost");
    var late = await Seed.HostAsync("Late Cohost");

    foreach (var part in new[] { p1, p2, p3 })
    {
      await Seed.HostOnPartAsync(part, primary, role: 0);
    }

    await Seed.HostOnPartAsync(p1, early, role: 1);
    await Seed.HostOnPartAsync(p2, early, role: 1);
    await Seed.HostOnPartAsync(p3, late, role: 1);

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    block.Should().Contain(P("commish", "[[Pat Primary]]") + "\n");
    block
      .Should()
      .Contain(P("cocommish", "[[Early Cohost]] (Parts I and II)<br>[[Late Cohost]] (Part III)"));
  }

  // -------------------------------------------------------------------------
  // Trivia
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldRenderWinnerFirstScore_WhenExactlyTwoCompetitorsPlayedTriviaAsync()
  {
    var draft = await Seed.DraftAsync("Trivia Two", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var bob = await Seed.DrafterAsync("Bob Brown");
    await Seed.ParticipantAsync(part, alice);
    await Seed.ParticipantAsync(part, bob);
    await Seed.TriviaAsync(part, position: 2, questionsWon: 2, participantId: bob.Id);
    await Seed.TriviaAsync(part, position: 1, questionsWon: 3, participantId: alice.Id);

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    block.Should().Contain(P("trivia", "Alice 3-2"));
  }

  [Fact]
  public async Task Export_ShouldRenderTheTopTwoPlacings_WhenThreeOrMoreCompetitorsPlayedTriviaAsync()
  {
    var draft = await Seed.DraftAsync("Trivia Three", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var bob = await Seed.DrafterAsync("Bob Brown");
    var carol = await Seed.DrafterAsync("Carol Clark");
    foreach (var drafter in new[] { alice, bob, carol })
    {
      await Seed.ParticipantAsync(part, drafter);
    }

    await Seed.TriviaAsync(part, position: 1, questionsWon: 4, participantId: carol.Id);
    await Seed.TriviaAsync(part, position: 2, questionsWon: 2, participantId: alice.Id);
    await Seed.TriviaAsync(part, position: 3, questionsWon: 1, participantId: bob.Id);

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    // The trailing "\n" proves the third placing (Bob) is not appended to the trivia line.
    block.Should().Contain(P("trivia", "1st: Carol<br>2nd: Alice") + "\n");
  }

  [Fact]
  public async Task Export_ShouldRenderOneBoldPartBlockPerPart_JoinedByDoubleBreaks_WhenMultiPartAsync()
  {
    var draft = await Seed.DraftAsync("Trivia Multi", episodeNumber: 1);
    var p1 = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var p2 = await Seed.PartAsync(draft, 2, mainFeed: Date(2024, 1, 8));
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var bob = await Seed.DrafterAsync("Bob Brown");
    foreach (var part in new[] { p1, p2 })
    {
      await Seed.ParticipantAsync(part, alice);
      await Seed.ParticipantAsync(part, bob);
    }

    await Seed.TriviaAsync(p1, position: 1, questionsWon: 3, participantId: alice.Id);
    await Seed.TriviaAsync(p1, position: 2, questionsWon: 2, participantId: bob.Id);
    await Seed.TriviaAsync(p2, position: 1, questionsWon: 4, participantId: bob.Id);
    await Seed.TriviaAsync(p2, position: 2, questionsWon: 1, participantId: alice.Id);

    var block = TemplateBlock(await ExportDraftPageAsync(draft));

    block
      .Should()
      .Contain(P("trivia", "'''Part I'''<br>Alice 3-2<br><br>'''Part II'''<br>Bob 4-1"));
  }

  // -------------------------------------------------------------------------
  // Intro and structure
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldWriteTheIntroWithOrdinalEpisodeDraftersAndLandedPickCountAsync()
  {
    var draft = await Seed.DraftAsync("Intro Draft", episodeNumber: 390);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 11, 1));
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var bob = await Seed.DrafterAsync("Bob Brown");
    var aliceRow = await Seed.ParticipantAsync(part, alice);
    var bobRow = await Seed.ParticipantAsync(part, bob);

    await Seed.PickAsync(part, aliceRow, await Seed.MovieAsync("Landed"), 7, 1);
    var vetoed = await Seed.PickAsync(part, bobRow, await Seed.MovieAsync("Vetoed"), 6, 2);
    var saved = await Seed.PickAsync(part, aliceRow, await Seed.MovieAsync("Saved"), 5, 3);
    var removed = await Seed.PickAsync(part, bobRow, await Seed.MovieAsync("Removed"), 4, 4);
    await Seed.VetoAsync(vetoed, aliceRow, sequence: 1);
    await Seed.VetoAsync(saved, bobRow, sequence: 1, overriddenBy: aliceRow);
    await Seed.CommissionerOverrideAsync(removed);

    var page = await ExportDraftPageAsync(draft);

    // landed + saved (veto overridden) count; finally-vetoed and commissioner-overridden do not.
    page
      .Should()
      .Contain(
        "'''Intro Draft''' is the 390th episode of [[Screen Drafts]]. "
          + "[[Alice Anderson]] and [[Bob Brown]] drafted 2 movies."
      );
  }

  [Fact]
  public async Task Export_ShouldSayAnEpisode_WhenThereIsNoEpisodeNumberAsync()
  {
    var draft = await Seed.DraftAsync("Unnumbered");
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var aliceRow = await Seed.ParticipantAsync(part, alice);
    await Seed.PickAsync(part, aliceRow, await Seed.MovieAsync("Solo One"), 7, 1);
    await Seed.PickAsync(part, aliceRow, await Seed.MovieAsync("Solo Two"), 6, 2);

    var page = await ExportDraftPageAsync(draft);

    page
      .Should()
      .Contain(
        "'''Unnumbered''' is an episode of [[Screen Drafts]]. [[Alice Anderson]] drafted 2 movies."
      );
    TemplateBlock(page).Should().Contain(P("episodeNumber") + "\n");
  }

  [Fact]
  public async Task Export_ShouldPrecedeThePicksWithTheDraftedFilmsLine_AndOmitPartHeadingsForASinglePartAsync()
  {
    var draft = await Seed.DraftAsync("Single", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var aliceRow = await Seed.ParticipantAsync(part, await Seed.DrafterAsync("Alice Anderson"));
    await Seed.PickAsync(part, aliceRow, await Seed.MovieAsync("Film A"), 7, 1);

    var page = await ExportDraftPageAsync(draft);

    page.Should().Contain("The following films were drafted:\n\n7. [[Film A]] by [[Alice Anderson]]");
    page.Should().NotContain("==Part ");
  }

  [Fact]
  public async Task Export_ShouldAddAPartHeadingBeforeEachPartsPicks_WhenTheDraftHasSeveralPartsAsync()
  {
    var draft = await Seed.DraftAsync("Double", episodeNumber: 1);
    var p1 = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var p2 = await Seed.PartAsync(draft, 2, mainFeed: Date(2024, 1, 8));
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var a1 = await Seed.ParticipantAsync(p1, alice);
    var a2 = await Seed.ParticipantAsync(p2, alice);
    await Seed.PickAsync(p1, a1, await Seed.MovieAsync("First Half"), 7, 1);
    await Seed.PickAsync(p2, a2, await Seed.MovieAsync("Second Half"), 7, 1);

    var page = await ExportDraftPageAsync(draft);

    Between(page, "The following films were drafted:", "==Predictions==")
      .Should()
      .Be(
        "The following films were drafted:\n\n"
          + "==Part I==\n\n"
          + "7. [[First Half]] by [[Alice Anderson]]\n\n"
          + "==Part II==\n\n"
          + "7. [[Second Half]] by [[Alice Anderson]]\n\n"
      );
  }
}
