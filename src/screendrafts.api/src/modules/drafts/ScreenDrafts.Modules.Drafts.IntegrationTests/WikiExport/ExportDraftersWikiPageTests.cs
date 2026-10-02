namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// Golden-output tests for the body of a drafter page: the intro, guest-commissioner sentence,
/// draft history and bullets, footer, Patreon exclusion, and drafter selection.
/// </summary>
public sealed class ExportDraftersWikiPageTests(DraftsIntegrationTestWebAppFactory factory)
  : WikiExportIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Intro
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldIntroduceAGuestGmAndListDraftsInAirOrderAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");

    // Air order (Zulu, Mid, Alpha) differs from both alphabetical and creation order.
    var alpha = await Seed.DraftAsync("Alpha Draft", episodeNumber: 3);
    var zulu = await Seed.DraftAsync("Zulu Draft", episodeNumber: 1);
    var mid = await Seed.DraftAsync("Mid Draft", episodeNumber: 2);
    await Seed.ParticipantAsync(await Seed.PartAsync(alpha, 1, mainFeed: Date(2024, 3, 1)), matt);
    await Seed.ParticipantAsync(await Seed.PartAsync(zulu, 1, mainFeed: Date(2024, 1, 1)), matt);
    await Seed.ParticipantAsync(await Seed.PartAsync(mid, 1, mainFeed: Date(2024, 2, 1)), matt);

    var page = await ExportDrafterPageAsync(matt);

    page
      .Should()
      .Contain(
        "'''Matt Singer''' is a [[Guest G.M.]] on [[Screen Drafts]], "
          + "participating in the [[Zulu Draft]], [[Mid Draft]] and [[Alpha Draft]].\n\n"
      );
  }

  [Fact]
  public async Task Export_ShouldCallAConfiguredCommissionerADrafterAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    SetCommissioners(matt.Person.PublicId);
    var draft = await Seed.DraftAsync("Only Draft", episodeNumber: 1);
    await Seed.ParticipantAsync(await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1)), matt);

    var page = await ExportDrafterPageAsync(matt);

    page
      .Should()
      .Contain(
        "'''Matt Singer''' is a drafter on [[Screen Drafts]], participating in the [[Only Draft]].\n\n"
      );
    page.Should().NotContain("[[Guest G.M.]]");
  }

  [Fact]
  public async Task Export_ShouldMentionGuestCommissionerAndCoCommissionerRolesWithMatchingCategoriesAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    var host = await Seed.HostForPersonAsync(matt.Person);

    var led = await Seed.DraftAsync("Led Draft", episodeNumber: 1);
    var assisted = await Seed.DraftAsync("Assisted Draft", episodeNumber: 2);
    var ledPart = await Seed.PartAsync(led, 1, mainFeed: Date(2024, 1, 1));
    var assistedPart = await Seed.PartAsync(assisted, 1, mainFeed: Date(2024, 2, 1));
    await Seed.HostOnPartAsync(ledPart, host, role: 0);
    await Seed.HostOnPartAsync(assistedPart, host, role: 1);

    var page = await ExportDrafterPageAsync(matt);

    page
      .Should()
      .Contain(
        "Matt also served as [[Guest Commissioner]] for the [[Led Draft]] "
          + "and [[Guest Co-Commissioner]] for the [[Assisted Draft]].\n\n==Draft History=="
      );
    page.Should().Contain("[[Category:Guest Commissioner]]");
    page.Should().Contain("[[Category:Guest Co-Commissioner]]");
  }

  [Fact]
  public async Task Export_ShouldNotMentionOrCategoriseHosting_ForAConfiguredCommissionerAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    SetCommissioners(matt.Person.PublicId);
    var host = await Seed.HostForPersonAsync(matt.Person);
    var draft = await Seed.DraftAsync("Hosted Draft", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    await Seed.HostOnPartAsync(part, host, role: 0);

    var page = await ExportDrafterPageAsync(matt);

    page.Should().NotContain("also served as");
    page.Should().NotContain("[[Category:Guest Commissioner]]");
    page.Should().NotContain("[[Category:Guest Co-Commissioner]]");
  }

  // -------------------------------------------------------------------------
  // Draft history and bullets
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldGroupHistoryByYearAscending_WithPartSuffixedHeadingsForMultiPartDraftsAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");

    var multi = await Seed.DraftAsync("Alpha Draft", episodeNumber: 2);
    var single = await Seed.DraftAsync("Zulu Draft", episodeNumber: 1);
    var multiOne = await Seed.PartAsync(multi, 1, mainFeed: Date(2024, 2, 1));
    var multiTwo = await Seed.PartAsync(multi, 2, mainFeed: Date(2024, 2, 8));
    var singlePart = await Seed.PartAsync(single, 1, mainFeed: Date(2023, 5, 1));
    await Seed.ParticipantAsync(multiOne, matt);
    await Seed.ParticipantAsync(multiTwo, matt);
    await Seed.ParticipantAsync(singlePart, matt);

    var page = await ExportDrafterPageAsync(matt);

    Between(page, "==Draft History==\n", "\n\n")
      .Should()
      .Be(
        string.Join(
          "\n",
          "==Draft History==",
          "==2023==",
          "===[[Zulu Draft]]===",
          "==2024==",
          "===[[Alpha Draft]] Part I===",
          "===[[Alpha Draft]] Part II==="
        )
      );
  }

  [Fact]
  public async Task Export_ShouldRenderPickAndVetoBulletsInPlayOrderAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    var bob = await Seed.DrafterAsync("Bob Brown");
    var carol = await Seed.DrafterAsync("Carol Clark");
    var draft = await Seed.DraftAsync("Bullets Draft", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var m = await Seed.ParticipantAsync(part, matt);
    var b = await Seed.ParticipantAsync(part, bob);
    var c = await Seed.ParticipantAsync(part, carol);

    // Created out of play order so the bullets must be sorted, with Matt's own picks and the
    // vetoes he issued interleaved by the play order of the pick they target.
    var removed = await Seed.PickAsync(part, m, await Seed.MovieAsync("Removed Film"), 3, 5);
    var carolPick = await Seed.PickAsync(part, c, await Seed.MovieAsync("Carol Film"), 2, 6);
    var saved = await Seed.PickAsync(part, m, await Seed.MovieAsync("Saved Film"), 4, 4);
    var vetoed = await Seed.PickAsync(part, m, await Seed.MovieAsync("Vetoed Film"), 5, 3);
    var bobPick = await Seed.PickAsync(part, b, await Seed.MovieAsync("Bob Film"), 6, 2);
    await Seed.PickAsync(part, m, await Seed.MovieAsync("Landed Film"), 7, 1);

    await Seed.CommissionerOverrideAsync(removed);
    await Seed.VetoAsync(carolPick, m, sequence: 1, overriddenBy: b);
    await Seed.VetoAsync(saved, b, sequence: 1, overriddenBy: c);
    await Seed.VetoAsync(vetoed, b, sequence: 1);
    await Seed.VetoAsync(bobPick, m, sequence: 1);

    var page = await ExportDrafterPageAsync(matt);

    Between(page, "==Draft History==\n", "\n\n")
      .Should()
      .Be(
        string.Join(
          "\n",
          "==Draft History==",
          "==2024==",
          "===[[Bullets Draft]]===",
          "* [[Landed Film]] at No. 7",
          "* Vetoed [[Bob Film]] drafted by [[Bob Brown]] at No. 6",
          "* <s>[[Vetoed Film]] at No. 5</s> vetoed by [[Bob Brown]]",
          "* [[Saved Film]] at No. 4 <s>vetoed by [[Bob Brown]]</s> veto overridden by [[Carol Clark]]",
          "* <s>[[Removed Film]] at No. 3</s> removed via [[Commissioner Override]]",
          "* Vetoed [[Carol Film]] drafted by [[Carol Clark]] at No. 2 <!-- veto overridden by Bob Brown -->"
        )
      );
  }

  // -------------------------------------------------------------------------
  // Footer
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldEndWithDefaultSortAndCategoriesInTheDocumentedOrderAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    ReportingApi.SetHonorific(matt.Id, "MVP");
    var host = await Seed.HostForPersonAsync(matt.Person);
    var led = await Seed.DraftAsync("Led Draft", episodeNumber: 1);
    var assisted = await Seed.DraftAsync("Assisted Draft", episodeNumber: 2);
    var ledPart = await Seed.PartAsync(led, 1, mainFeed: Date(2024, 1, 1));
    var assistedPart = await Seed.PartAsync(assisted, 1, mainFeed: Date(2024, 2, 1));
    await Seed.HostOnPartAsync(ledPart, host, role: 0);
    await Seed.HostOnPartAsync(assistedPart, host, role: 1);
    await Seed.ParticipantAsync(ledPart, matt);

    var page = await ExportDrafterPageAsync(matt);

    page
      .Should()
      .EndWith(
        "\n\n{{DEFAULTSORT:Singer, Matt}}\n"
          + "[[Category:Drafters]]\n"
          + "[[Category:MVPs]]\n"
          + "[[Category:Guest Co-Commissioner]]\n"
          + "[[Category:Guest Commissioner]]"
      );
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Export_ShouldOmitDefaultSort_WhenTheLastNameIsBlankAsync(string lastName)
  {
    var cher = await Seed.DrafterAsync("Cher", firstName: "Cher", lastName: lastName);

    var page = await ExportDrafterPageAsync(cher);

    page.Should().NotContain("DEFAULTSORT");
    page.Should().EndWith("[[Category:Drafters]]");
  }

  // -------------------------------------------------------------------------
  // No Patreon
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldIgnorePatreonAppearancesInHistoryAndEveryStatAsync()
  {
    var matt = await Seed.DrafterAsync("Matt Singer");
    var bob = await Seed.DrafterAsync("Bob Brown");

    // Main-feed draft: one landed pick, one veto used.
    var main = await Seed.DraftAsync("Main Draft", episodeNumber: 1);
    var mainPart = await Seed.PartAsync(main, 1, mainFeed: Date(2024, 1, 1));
    var mainRow = await Seed.ParticipantAsync(mainPart, matt, vetoesUsed: 1);
    await Seed.PickAsync(mainPart, mainRow, await Seed.MovieAsync("Main Film"), 7, 1);

    // Patreon-only draft: picks (one vetoed), vetoes used, everything the stats could count.
    var secret = await Seed.DraftAsync("Patreon Only Draft");
    var secretPart = await Seed.PartAsync(secret, 1, patreon: Date(2024, 2, 1));
    var secretRow = await Seed.ParticipantAsync(secretPart, matt, vetoesUsed: 4, vetoesRollingIn: 3);
    var bobSecret = await Seed.ParticipantAsync(secretPart, bob);
    await Seed.PickAsync(secretPart, secretRow, await Seed.MovieAsync("Secret One"), 7, 1);
    var secretVetoed = await Seed.PickAsync(secretPart, secretRow, await Seed.MovieAsync("Secret Two"), 6, 2);
    await Seed.VetoAsync(secretVetoed, bobSecret, sequence: 1);

    // Mixed draft: part I is main feed, part II is Patreon-only.
    var mixed = await Seed.DraftAsync("Mixed Draft", episodeNumber: 2);
    var mixedOne = await Seed.PartAsync(mixed, 1, mainFeed: Date(2024, 3, 1));
    var mixedTwo = await Seed.PartAsync(mixed, 2, patreon: Date(2024, 3, 8));
    var mixedOneRow = await Seed.ParticipantAsync(mixedOne, matt);
    var mixedTwoRow = await Seed.ParticipantAsync(mixedTwo, matt, vetoesUsed: 2);
    await Seed.PickAsync(mixedOne, mixedOneRow, await Seed.MovieAsync("Mixed Public"), 7, 1);
    await Seed.PickAsync(mixedTwo, mixedTwoRow, await Seed.MovieAsync("Mixed Patreon"), 7, 1);

    var page = await ExportDrafterPageAsync(matt);
    var block = TemplateBlock(page);

    block.Should().Contain("  | drafts=2\n");
    block.Should().Contain("  | last_draft=[[Mixed Draft]]\n");
    block.Should().Contain("  | filmsdrafted=2\n");
    block.Should().Contain("  | picksvetoed=0\n");
    block.Should().Contain("  | vetoused=1\n");
    block.Should().Contain("  | rollover=\n", "the Patreon participation's spare vetoes must not count");
    page.Should().Contain("[[Main Film]]").And.Contain("[[Mixed Public]]");
    page.Should().NotContain("Patreon Only Draft");
    page.Should().NotContain("Secret One").And.NotContain("Secret Two");
    page.Should().NotContain("Mixed Patreon");
  }

  // -------------------------------------------------------------------------
  // Selection
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Export_ShouldSkipUnknownDrafterIds_AndReturnPagesInRequestedOrderAsync()
  {
    var alice = await Seed.DrafterAsync("Alice Anderson");
    var bob = await Seed.DrafterAsync("Bob Brown");

    var result = await ExportDraftersAsync(bob.PublicId, "dr_doesnotexist000", alice.PublicId);

    result.IsSuccess.Should().BeTrue();
    PageTitles(result.Value).Should().Equal("Bob Brown", "Alice Anderson");
    result.Value.PageCount.Should().Be(2);
  }

  [Fact]
  public async Task Export_ShouldExportADrafterOnce_WhenTheIdIsRepeatedAsync()
  {
    var alice = await Seed.DrafterAsync("Alice Anderson");

    var result = await ExportDraftersAsync(alice.PublicId, alice.PublicId);

    result.IsSuccess.Should().BeTrue();
    PageTitles(result.Value).Should().Equal("Alice Anderson");
  }

  [Fact]
  public async Task Export_ShouldReturnNoDraftersFound_WhenNoRequestedIdResolvesAsync()
  {
    var result = await ExportDraftersAsync("dr_doesnotexist000", "dr_alsomissing0000");

    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle().Which.Should().Be(WikiExportErrors.NoDraftersFound);
  }
}
