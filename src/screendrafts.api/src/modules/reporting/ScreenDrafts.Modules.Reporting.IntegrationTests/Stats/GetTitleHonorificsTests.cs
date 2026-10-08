using static ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions.StatsSeeder;

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Stats;

public sealed class GetTitleHonorificsTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly DateOnly _jan1 = new(2026, 1, 1);
  private static readonly DateOnly _feb1 = new(2026, 2, 1);
  private static readonly DateOnly _mar1 = new(2026, 3, 1);
  private static readonly DateOnly _apr1 = new(2026, 4, 1);

  /// <summary>
  /// Release order: Epic part 1 (Jan 1), Between (Feb 1), Epic part 2 (Mar 1), Later (Apr 1). Epic is episode 10 for
  /// both parts, Between is 11 and Later is 12, so episode order disagrees with release order.
  /// Epic p1: Heat, Casino | Between: Heat, Alien, Dune | Epic p2: Heat | Later: Casino, Dune, Alien (in play order).
  /// </summary>
  private async Task SeedAsync(bool interloper = false)
  {
    var seed = new StatsSeeder(DbContext);

    var epic = seed.Draft("Epic", totalParts: 2);
    var epic1 = epic.Part(index: 1, episode: 10, mainFeed: _jan1);
    epic1.Pick("Heat", 5, 1, playOrder: 1);
    epic1.Pick("Casino", 4, 2, playOrder: 2);

    if (interloper)
    {
      // A released part with no picks still takes a release rank.
      seed.Draft("Interloper").Part(episode: 9, mainFeed: _jan1.AddDays(14));
    }

    var between = seed.Draft("Between").Part(episode: 11, mainFeed: _feb1);
    between.Pick("Heat", 3, 1, playOrder: 1);
    between.Pick("Alien", 2, 2, playOrder: 2);
    between.Pick("Dune", 1, 3, playOrder: 3);

    var epic2 = epic.Part(index: 2, episode: 10, mainFeed: _mar1);
    epic2.Pick("Heat", 2, 1, playOrder: 1);

    var later = seed.Draft("Later").Part(episode: 12, mainFeed: _apr1);
    later.Pick("Casino", 1, 1, playOrder: 1);
    later.Pick("Dune", 2, 2, playOrder: 2);
    later.Pick("Alien", 3, 3, playOrder: 3);

    await seed.SaveAsync(TestContext.Current.CancellationToken);
  }

  private async Task<GetTitleHonorificsResponse> TitlesAsync(
    string level = TitleHonorificLevels.MarqueeOfFame,
    Func<GetTitleHonorificsQuery, GetTitleHonorificsQuery>? tweak = null
  )
  {
    var query = new GetTitleHonorificsQuery { Level = level, IncludeAll = false };
    var result = await Sender.Send(
      tweak?.Invoke(query) ?? query,
      TestContext.Current.CancellationToken
    );

    result.IsSuccess.Should().BeTrue();
    return result.Value;
  }

  // -------------------------------------------------------------------------
  // Join order and ordering by part release date
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNumberTitlesInJoinOrder_ByPartReleaseDateAsync()
  {
    await SeedAsync();

    var response = await TitlesAsync(tweak: q => q with { Sort = "oldest" });

    // Heat joins on Between (Feb 1). Casino, Dune and Alien join on Later (Apr 1), in play order.
    response
      .Titles.Select(t => (t.Number, t.Title))
      .Should()
      .Equal((1, "Heat"), (2, "Casino"), (3, "Dune"), (4, "Alien"));
  }

  [Fact]
  public async Task Handle_ShouldJoinOnTheSecondRelease_NotTheSecondEpisodeNumberAsync()
  {
    await SeedAsync();

    var heat = (await TitlesAsync()).Titles.Single(t => t.Title == "Heat");

    // By episode, Heat's 2nd appearance would be Epic part 2. By release date it is Between.
    heat.JoinedDraftTitle.Should().Be("Between");
    heat.JoinedOn.Should().Be("2026-02-01");
    heat.JoinedEpisode.Should().Be(11);
    heat.FirstDraftTitle.Should().Be("Epic");
    heat.FirstOn.Should().Be("2026-01-01");
    heat.Appearances.Select(a => (a.DraftTitle, a.PartIndex))
      .Should()
      .Equal(("Epic", 1), ("Between", 1), ("Epic", 2));
  }

  [Fact]
  public async Task Handle_ShouldJoinHatTrickOnThePartThatReachesThreeAsync()
  {
    await SeedAsync();

    var response = await TitlesAsync(TitleHonorificLevels.HatTrick);

    var heat = response.Titles.Should().ContainSingle().Subject;
    heat.Title.Should().Be("Heat");
    heat.Number.Should().Be(1);
    heat.AppearanceCount.Should().Be(3);
    heat.JoinedDraftTitle.Should().Be("Epic");
    heat.JoinedPartIndex.Should().Be(2);
    heat.JoinedTotalParts.Should().Be(2);
    heat.JoinedOn.Should()
      .Be("2026-03-01", "the date is the part's own, not the draft's first part");
  }

  [Fact]
  public async Task Handle_ShouldNumberTitlesJoiningOnTheSamePartByPlayOrderAsync()
  {
    await SeedAsync();

    var response = await TitlesAsync(tweak: q => q with { Sort = "oldest" });

    // Alphabetical would be Alien, Casino, Dune. Play order on Later is Casino, Dune, Alien.
    response.Titles.Skip(1).Select(t => t.Title).Should().Equal("Casino", "Dune", "Alien");
  }

  // -------------------------------------------------------------------------
  // Gaps
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldMeasureGapEpisodesByReleaseRank_AndGapDaysByDateAsync()
  {
    await SeedAsync();

    var marquee = (await TitlesAsync()).Titles.ToDictionary(t => t.Title);

    marquee["Heat"]
      .GapEpisodes.Should()
      .Be(1, "Between is the very next release after Epic part 1");
    marquee["Heat"].GapDays.Should().Be(31);
    marquee["Casino"].GapEpisodes.Should().Be(3);
    marquee["Casino"].GapDays.Should().Be(90);
    marquee["Alien"].GapEpisodes.Should().Be(2);
    marquee["Alien"].GapDays.Should().Be(59);

    var hatTrick = (await TitlesAsync(TitleHonorificLevels.HatTrick)).Titles.Single();
    hatTrick.GapEpisodes.Should().Be(2);
    hatTrick.GapDays.Should().Be(59);
  }

  [Fact]
  public async Task Handle_ShouldCountEveryReleasedPartInTheGap_EvenOnesWithNoPicksAsync()
  {
    await SeedAsync(interloper: true);

    var heat = (await TitlesAsync()).Titles.Single(t => t.Title == "Heat");

    // Epic p1 = 1, Interloper = 2, Between = 3.
    heat.GapEpisodes.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldJoinOnPartFour_WithThatPartsDateAndAGapInReleasedPartsAsync()
  {
    var seed = new StatsSeeder(DbContext);
    seed.Draft("Solo").Part(episode: 5, mainFeed: _apr1).Pick("Zodiac", 1, 1);

    var saga = seed.Draft("Saga", totalParts: 4);
    for (var part = 1; part <= 4; part++)
    {
      var p = saga.Part(
        index: part,
        episode: 20,
        mainFeed: new DateOnly(2026, 5, 1).AddDays(7 * (part - 1))
      );
      p.Pick(part == 4 ? "Zodiac" : $"Filler {part}", 1, 1);
    }

    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var zodiac = (await TitlesAsync()).Titles.Single();

    zodiac.JoinedDraftTitle.Should().Be("Saga");
    zodiac.JoinedPartIndex.Should().Be(4);
    zodiac.JoinedTotalParts.Should().Be(4);
    zodiac.JoinedOn.Should().Be("2026-05-22");
    zodiac.GapEpisodes.Should().Be(4, "Solo is release 1 and Saga part 4 is release 5");
    zodiac.GapDays.Should().Be(51);
  }

  [Fact]
  public async Task Handle_ShouldReturnNullGaps_WhenTheJoiningPartHasNoMainFeedReleaseAsync()
  {
    var seed = new StatsSeeder(DbContext);
    seed.Draft("Released", policy: 0).Part(episode: 1, mainFeed: _jan1).Pick("Heat", 1, 1);
    seed.Draft("Unreleased", policy: 0).Part(episode: 2).Pick("Heat", 1, 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var heat = (await TitlesAsync()).Titles.Single();

    heat.JoinedDraftTitle.Should().Be("Unreleased");
    heat.GapEpisodes.Should().BeNull();
    heat.GapDays.Should().BeNull();
    heat.JoinedOn.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Counts, search, sort and paging
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldListTheSizeOfEveryLevelAsync()
  {
    await SeedAsync();

    var counts = (await TitlesAsync()).Counts;

    counts
      .Select(c => (c.Code, c.Count))
      .Should()
      .Equal(
        ("marquee-of-fame", 4),
        ("hat-trick", 1),
        ("grand-slam", 0),
        ("high-five", 0),
        ("6-drafts", 0),
        ("7-drafts", 0),
        ("8-drafts", 0),
        ("9-drafts", 0),
        ("10-drafts", 0)
      );
  }

  [Fact]
  public async Task Handle_ShouldKeepJoinNumbers_WhenSearchingAsync()
  {
    await SeedAsync();

    var response = await TitlesAsync(tweak: q => q with { Search = "CAS" });

    var casino = response.Titles.Should().ContainSingle().Subject;
    casino.Title.Should().Be("Casino");
    casino.Number.Should().Be(2);
    response.TotalMatching.Should().Be(1);
    response.Counts[0].Count.Should().Be(4, "counts ignore the search");
  }

  [Fact]
  public async Task Handle_ShouldSortNewestFirstByDefault_AndSupportTheOtherSortsAsync()
  {
    await SeedAsync();

    (await TitlesAsync())
      .Titles.Select(t => t.Title)
      .Should()
      .Equal("Alien", "Dune", "Casino", "Heat");
    (await TitlesAsync(tweak: q => q with { Sort = "alphabetical" }))
      .Titles.Select(t => t.Title)
      .Should()
      .Equal("Alien", "Casino", "Dune", "Heat");
    (await TitlesAsync(tweak: q => q with { Sort = "appearances" }))
      .Titles[0]
      .Title.Should()
      .Be("Heat");
  }

  [Fact]
  public async Task Handle_ShouldPageResults_AndReportTotalsAsync()
  {
    await SeedAsync();

    var second = await TitlesAsync(tweak: q => q with { Sort = "oldest", Page = 2, PageSize = 3 });

    second.Titles.Select(t => t.Number).Should().Equal(4);
    second.TotalMatching.Should().Be(4);
    second.TotalPages.Should().Be(2);
    second.Page.Should().Be(2);
    second.PageSize.Should().Be(3);
    second.Level.Should().Be("marquee-of-fame");
    second.MinAppearances.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldReturnNotFound_WhenTheLevelIsUnknownAsync()
  {
    var result = await Sender.Send(
      new GetTitleHonorificsQuery { Level = "six-pack", IncludeAll = false },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue();
    result.Error!.Type.Should().Be(ErrorType.NotFound);
  }

  [Fact]
  public async Task Handle_ShouldReturnAProblem_WhenTheSortIsUnknownAsync()
  {
    var result = await Sender.Send(
      new GetTitleHonorificsQuery
      {
        Level = "hat-trick",
        Sort = "random",
        IncludeAll = false,
      },
      TestContext.Current.CancellationToken
    );

    result.Error!.Type.Should().Be(ErrorType.Problem);
  }

  [Fact]
  public async Task Handle_ShouldReturnAnEmptyListWithOnePage_WhenThereAreNoFactsAsync()
  {
    var response = await TitlesAsync();

    response.Titles.Should().BeEmpty();
    response.TotalPages.Should().Be(1);
    response.Counts.Should().OnlyContain(c => c.Count == 0);
  }

  // -------------------------------------------------------------------------
  // Scope
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldExcludeNonCanonicalDrafts_UnlessIncludeAllAsync()
  {
    var seed = new StatsSeeder(DbContext);
    seed.Draft("Canon", policy: 0).Part(episode: 1, mainFeed: _jan1).Pick("Heat", 1, 1);
    seed.Draft("Never canonical", policy: 1).Part(episode: 2, mainFeed: _feb1).Pick("Heat", 1, 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    (await TitlesAsync()).Titles.Should().BeEmpty();

    var all = await TitlesAsync(tweak: q => q with { IncludeAll = true });
    all.Titles.Should().ContainSingle();
    all.IncludesNonCanonical.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Appearance rules (regressions from real data)
  // -------------------------------------------------------------------------

  /// <summary>Two drafts released a week apart, each holding one "Elf" pick configured by the caller.</summary>
  private async Task SeedElfAsync(Action<PickSeed> first, Action<PickSeed> second)
  {
    var seed = new StatsSeeder(DbContext);
    first(seed.Draft("One").Part(episode: 1, mainFeed: _jan1).Pick("Elf", 1, 1));
    second(seed.Draft("Two").Part(episode: 2, mainFeed: _jan1.AddDays(7)).Pick("Elf", 1, 1));
    await seed.SaveAsync(TestContext.Current.CancellationToken);
  }

  [Fact]
  public async Task Handle_ShouldCountTwoAppearances_WhenATitleIsPickedInTwoPartsAndTheFirstIsUnvetoedAsync()
  {
    await SeedElfAsync(_ => { }, _ => { });

    var elf = (await TitlesAsync()).Titles.Should().ContainSingle().Subject;

    elf.AppearanceCount.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldCountOneAppearance_WhenTheFirstPickWasVetoedAsync()
  {
    await SeedElfAsync(p => p.Veto(DrafterKind, 2), _ => { });

    (await TitlesAsync()).Titles.Should().BeEmpty("one landed appearance is below the Marquee");
  }

  [Fact]
  public async Task Handle_ShouldCountOneAppearance_WhenTheTitleIsPickedTwiceInTheSamePartAndTheFirstWasVetoedAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var part = seed.Draft("One").Part(episode: 1, mainFeed: _jan1);
    part.Pick("Elf", 1, 1).Veto(DrafterKind, 2);
    part.Pick("Elf", 1, 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    (await TitlesAsync()).Titles.Should().BeEmpty();
    (await TitlesAsync(tweak: q => q with { IncludeAll = true })).Counts[0].Count.Should().Be(0);
  }

  [Fact]
  public async Task Handle_ShouldCountOneAppearance_WhenTheTitleIsPickedTwiceInTheSamePartAndBothLandedAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var part = seed.Draft("One").Part(episode: 1, mainFeed: _jan1);
    part.Pick("Elf", 1, 1);
    part.Pick("Elf", 2, 2);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    (await TitlesAsync()).Titles.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldCountAPickSavedByAVetoOverrideAsAnAppearanceAsync()
  {
    await SeedElfAsync(p => p.Veto(DrafterKind, 2, overridden: true, overriddenBy: 3), _ => { });

    (await TitlesAsync()).Titles.Should().ContainSingle().Which.AppearanceCount.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldNotCountAPick_WhenAnOverrideWasItselfVetoedAgainAsync()
  {
    // Veto 1 was overridden, then veto 2 (sequence 2, not overridden) stood: the pick did not land.
    await SeedElfAsync(
      p => p.Veto(DrafterKind, 2, overridden: true, overriddenBy: 3).Veto(DrafterKind, 4),
      _ => { }
    );

    (await TitlesAsync()).Titles.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldNotCountAPickRemovedByCommissionerOverrideAsync()
  {
    await SeedElfAsync(p => p.Removed(), _ => { });

    (await TitlesAsync()).Titles.Should().BeEmpty();
  }
}
