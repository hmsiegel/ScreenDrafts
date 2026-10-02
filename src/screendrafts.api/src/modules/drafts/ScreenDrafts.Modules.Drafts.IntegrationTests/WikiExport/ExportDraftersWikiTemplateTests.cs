namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// Golden-output tests for the <c>{{Drafter}}</c> template block: its parameter order, the stats
/// (main feed only), the honorific banner and the social links.
/// </summary>
public sealed class ExportDraftersWikiTemplateTests(DraftsIntegrationTestWebAppFactory factory)
  : WikiExportIntegrationTest(factory)
{
  private const string Twitter = "[[File:Twitter.png|50px|alt=Twitter|link=https://twitter.com/";

  // -------------------------------------------------------------------------
  // Template block and stats
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldRenderTheTemplateBlockInTheDocumentedOrder_WithMainFeedStatsAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer", twitter: "@mattsinger");
    ReportingApi.SetHonorific(matt.Id, "MVP");
    var bob = await Seed.DrafterAsync("Bob Brown");
    var carol = await Seed.DrafterAsync("Carol Clark");

    // Draft One: single part. Matt has 4 picks: one landed, one finally vetoed, one saved by an
    // override, one removed by the commissioner.
    var one = await Seed.DraftAsync("Draft One", episodeNumber: 1);
    var p1 = await Seed.PartAsync(one, 1, mainFeed: Date(2023, 1, 1));
    var m1 = await Seed.ParticipantAsync(p1, matt, vetoesUsed: 1, startingVetoes: 1);
    var b1 = await Seed.ParticipantAsync(p1, bob);
    var c1 = await Seed.ParticipantAsync(p1, carol);
    await Seed.PickAsync(p1, m1, await Seed.MovieAsync("L1"), position: 7, playOrder: 1);
    var v1 = await Seed.PickAsync(p1, m1, await Seed.MovieAsync("V1"), position: 6, playOrder: 2);
    var s1 = await Seed.PickAsync(p1, m1, await Seed.MovieAsync("S1"), position: 5, playOrder: 3);
    var r1 = await Seed.PickAsync(p1, m1, await Seed.MovieAsync("R1"), position: 4, playOrder: 4);
    await Seed.VetoAsync(v1, b1, sequence: 1);
    await Seed.VetoAsync(s1, b1, sequence: 1, overriddenBy: c1);
    await Seed.CommissionerOverrideAsync(r1);

    // Draft Two: two parts, so Matt's two participations count as ONE draft.
    var two = await Seed.DraftAsync("Draft Two", episodeNumber: 2);
    var p2a = await Seed.PartAsync(two, 1, mainFeed: Date(2024, 2, 1));
    var p2b = await Seed.PartAsync(two, 2, mainFeed: Date(2024, 2, 8));
    var m2a = await Seed.ParticipantAsync(p2a, matt, vetoesUsed: 1);
    var b2a = await Seed.ParticipantAsync(p2a, bob);
    var c2a = await Seed.ParticipantAsync(p2a, carol);

    // The latest participation (part II) carries one unused rolled-in veto => rollover=Y.
    var m2b = await Seed.ParticipantAsync(p2b, matt, vetoesRollingIn: 1);
    await Seed.PickAsync(p2a, m2a, await Seed.MovieAsync("L2"), position: 7, playOrder: 1);
    var v2 = await Seed.PickAsync(p2a, m2a, await Seed.MovieAsync("V2"), position: 6, playOrder: 3);
    var bobPick = await Seed.PickAsync(p2a, b2a, await Seed.MovieAsync("BP"), position: 5, playOrder: 2);
    await Seed.VetoAsync(v2, c2a, sequence: 1);
    await Seed.VetoAsync(bobPick, m2a, sequence: 1);
    await Seed.PickAsync(p2b, m2b, await Seed.MovieAsync("L3"), position: 7, playOrder: 1);

    var block = TemplateBlock(await ExportDrafterPageAsync(matt));

    block
      .Should()
      .Be(
        string.Join(
          "\n",
          "{{Drafter",
          "  | image1=",
          "  | honor=MVP Banner.jpg",
          "  | drafts=2",
          "  | first_draft=[[Draft One]]",
          "  | last_draft=[[Draft Two]]",
          "  | filmsdrafted=4",
          "  | picksvetoed=2",
          "  | vetoused=2",
          "  | rollover=Y",
          "  | override=",
          $"  | twitter={Twitter}mattsinger]]",
          "  | letterboxd=",
          "  | instagram=",
          "  | bluesky=",
          "}}"
        )
      );
  }

  [Fact]
  public async Task Export_ShouldTakeFirstAndLastDraftFromAirDate_NotCreationOrderAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");

    // Created newest-first on purpose.
    var late = await Seed.DraftAsync("Late Draft", episodeNumber: 2);
    var early = await Seed.DraftAsync("Early Draft", episodeNumber: 1);
    var lateRow = await Seed.PartAsync(late, 1, mainFeed: Date(2024, 6, 1));
    var earlyRow = await Seed.PartAsync(early, 1, mainFeed: Date(2023, 6, 1));
    await Seed.ParticipantAsync(lateRow, matt);
    await Seed.ParticipantAsync(earlyRow, matt);

    var block = TemplateBlock(await ExportDrafterPageAsync(matt));

    block.Should().Contain("  | first_draft=[[Early Draft]]\n");
    block.Should().Contain("  | last_draft=[[Late Draft]]\n");
  }

  [Theory]
  [InlineData(1, 0, 0, 0, true)]
  [InlineData(0, 1, 0, 0, true)]
  [InlineData(0, 0, 1, 0, true)]
  [InlineData(2, 0, 0, 1, true)]
  [InlineData(1, 1, 0, 1, true)]
  [InlineData(1, 0, 0, 1, false)]
  [InlineData(0, 0, 0, 0, false)]
  [InlineData(1, 1, 0, 2, false)]
  public async Task Export_ShouldSetRolloverFromStartingPlusRollingInPlusAwardedMinusUsedVetoesAsync(
    int starting,
    int rollingIn,
    int awarded,
    int used,
    bool expectRollover
  )
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    var draft = await Seed.DraftAsync("Only Draft", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    await Seed.ParticipantAsync(
      part,
      matt,
      vetoesUsed: used,
      startingVetoes: starting,
      vetoesRollingIn: rollingIn,
      awardedVetoes: awarded
    );

    var block = TemplateBlock(await ExportDrafterPageAsync(matt));

    block.Should().Contain($"  | rollover={(expectRollover ? "Y" : string.Empty)}\n");
  }

  [Theory]
  [InlineData(1, 0, 0, true)]
  [InlineData(0, 1, 0, true)]
  [InlineData(2, 0, 1, true)]
  [InlineData(1, 0, 1, false)]
  [InlineData(0, 0, 0, false)]
  public async Task Export_ShouldSetOverrideFromRollingInPlusAwardedMinusUsedVetoOverridesAsync(
    int rollingIn,
    int awarded,
    int used,
    bool expectOverride
  )
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    var draft = await Seed.DraftAsync("Only Draft", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    await Seed.ParticipantAsync(
      part,
      matt,
      vetoOverridesRollingIn: rollingIn,
      awardedVetoOverrides: awarded,
      vetoOverridesUsed: used
    );

    var block = TemplateBlock(await ExportDrafterPageAsync(matt));

    block.Should().Contain($"  | override={(expectOverride ? "Y" : string.Empty)}\n");
  }

  [Fact]
  public async Task Export_ShouldReadRolloverAndOverrideFromTheLatestParticipationOnly_NotAnEarlierOneAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    var earlier = await Seed.DraftAsync("Earlier Draft", episodeNumber: 1);
    var later = await Seed.DraftAsync("Later Draft", episodeNumber: 2);
    var earlierPart = await Seed.PartAsync(earlier, 1, mainFeed: Date(2023, 1, 1));
    var laterPart = await Seed.PartAsync(later, 1, mainFeed: Date(2024, 1, 1));

    // The earlier appearance has a spare veto and override; the latest has none left.
    await Seed.ParticipantAsync(
      earlierPart,
      matt,
      vetoesRollingIn: 1,
      vetoOverridesRollingIn: 1
    );
    await Seed.ParticipantAsync(
      laterPart,
      matt,
      vetoesRollingIn: 1,
      vetoesUsed: 1,
      vetoOverridesRollingIn: 1,
      vetoOverridesUsed: 1
    );

    var block = TemplateBlock(await ExportDrafterPageAsync(matt));

    block.Should().Contain("  | rollover=\n");
    block.Should().Contain("  | override=\n");
  }

  [Fact]
  public async Task Export_ShouldRenderZeroStatsAndBlankDrafts_WhenTheDrafterHasNoMainFeedAppearancesAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");

    var block = TemplateBlock(await ExportDrafterPageAsync(matt));

    block.Should().Contain("  | drafts=0\n");
    block.Should().Contain("  | first_draft=\n");
    block.Should().Contain("  | last_draft=\n");
    block.Should().Contain("  | filmsdrafted=0\n");
    block.Should().Contain("  | picksvetoed=0\n");
    block.Should().Contain("  | vetoused=0\n");
  }

  // -------------------------------------------------------------------------
  // Honorific
  // -------------------------------------------------------------------------

  // NOTE: the "{HonorificName} Banner.jpg" file naming is a provisional guess, not a verified
  // wiki convention. This test pins today's behaviour so a deliberate rename shows up as a diff.
  [Fact]
  public async Task Export_ShouldUseTheHonorificNameAsTheBannerFile_ProvisionalNamingAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    ReportingApi.SetHonorific(matt.Id, "Hall of Famer");

    var block = TemplateBlock(await ExportDrafterPageAsync(matt));

    block.Should().Contain("  | honor=Hall of Famer Banner.jpg\n");
  }

  [Fact]
  public async Task Export_ShouldLeaveHonorBlank_WhenTheReportingApiReturnsNoHonorificAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");

    var page = await ExportDrafterPageAsync(matt);

    TemplateBlock(page).Should().Contain("  | honor=\n");
    page.Should().NotContain("Banner.jpg");
    page.Should().NotContain("[[Category:s]]");
  }

  // -------------------------------------------------------------------------
  // Social handles
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldBuildEachSocialLinkFromTheTrimmedHandleWithoutALeadingAtSignAsync()
  {
    var alice = await Seed.DrafterAsync(
      "Alice Anderson",
      twitter: "@alice_t",
      letterboxd: "alice_l",
      instagram: "  @alice_i  ",
      bluesky: "alice.bsky.social"
    );

    var block = TemplateBlock(await ExportDrafterPageAsync(alice));

    block
      .Should()
      .Contain($"  | twitter={Twitter}alice_t]]\n")
      .And.Contain(
        "  | letterboxd=[[File:Letterboxd.png|50px|alt=Letterboxd|link=https://letterboxd.com/alice_l/]]\n"
      )
      .And.Contain(
        "  | instagram=[[File:Instagram.png|50px|alt=Instagram|link=https://www.instagram.com/alice_i/]]\n"
      )
      .And.Contain(
        "  | bluesky=[[File:Bluesky.png|50px|alt=Bluesky|link=https://bsky.app/profile/alice.bsky.social]]\n"
      );
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("@")]
  public async Task Export_ShouldLeaveASocialValueEmpty_WhenTheHandleIsMissingOrBlankAsync(
    string? handle
  )
  {
    var alice = await Seed.DrafterAsync(
      "Alice Anderson",
      twitter: handle,
      letterboxd: handle,
      instagram: handle,
      bluesky: handle
    );

    var block = TemplateBlock(await ExportDrafterPageAsync(alice));

    block
      .Should()
      .Contain("  | twitter=\n")
      .And.Contain("  | letterboxd=\n")
      .And.Contain("  | instagram=\n")
      .And.Contain("  | bluesky=\n");
  }
}
