using static ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions.StatsSeeder;

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Stats;

/// <summary>
/// Runs every loader query against an empty database and against seeded facts, so a column alias that
/// does not match its Dapper row type (or a SQL typo) fails here rather than in production.
/// </summary>
public sealed class StatsLoaderSmokeTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly DateOnly _jan1 = new(2026, 1, 1);

  private async Task<System.Data.Common.DbConnection> OpenAsync() =>
    await GetService<IDbConnectionFactory>().OpenConnectionAsync(TestContext.Current.CancellationToken);

  private async Task SeedAsync()
  {
    var seed = new StatsSeeder(DbContext);

    var one = seed.Draft("One", totalParts: 2);
    var p1 = one.Part(index: 1, episode: 1, mainFeed: _jan1);
    p1.Pick("Heat", 1, 1);
    p1.Pick("Alien", 2, 2).Veto(DrafterKind, 1);
    p1.Pick("Brazil", 2, 2).Veto(DrafterKind, 1, overridden: true, overriddenBy: 3);
    p1.Pick("Casino", 3, 1).Veto(CommunityKind);
    p1.Pick("Dune", 4, 2).Removed();
    p1.TeamPick("Elf", 5, team: 1, members: [3, 4]);
    p1.CommunityPick("Fargo", 6);

    var p2 = one.Part(index: 2, episode: 1, mainFeed: _jan1.AddDays(7));
    p2.Pick("Heat", 1, 1);

    seed.Draft("Two", type: "Mega", policy: 2).Part(episode: 2, subDraftIndex: 1).Pick("Heat", 1, 3);

    seed.MovieHonorific("m_heat", MovieHonorific.HatTrick, 3);

    await seed.SaveAsync(TestContext.Current.CancellationToken);
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task RecordBookDataLoader_ShouldRunAgainstAnEmptyDatabaseAsync(bool includeAll)
  {
    await using var connection = await OpenAsync();

    var data = await RecordBookDataLoader.LoadAsync(connection, includeAll, TestContext.Current.CancellationToken);

    data.Parts.Should().BeEmpty();
    data.DrafterDrafts.Should().BeEmpty();
    data.Media.Should().BeEmpty();
    data.PickSlots.Should().BeEmpty();
    data.VetoesStood.Should().Be(0);
    data.MarqueeOfFameTitles.Should().Be(0);
  }

  [Theory]
  [InlineData(false, 1, 2)]
  [InlineData(true, 2, 3)]
  public async Task RecordBookDataLoader_ShouldMapEveryColumn_WhenFactsExistAsync(bool includeAll, int drafts, int parts)
  {
    await SeedAsync();
    await using var connection = await OpenAsync();

    var data = await RecordBookDataLoader.LoadAsync(connection, includeAll, TestContext.Current.CancellationToken);

    data.Parts.Select(p => p.DraftId).Distinct().Should().HaveCount(drafts);
    data.Parts.Should().HaveCount(parts);
    data.Parts.Should().OnlyContain(p => p.DraftTitle.Length > 0 && p.PartPublicId.Length > 0);
    data.DrafterDrafts.Should().NotBeEmpty();
    data.DrafterDrafts.Should().OnlyContain(r => r.DrafterName.Length > 0);
    data.Media.Should().NotBeEmpty();
    data.VetoesStood.Should().Be(2); // Alien (D1) and the community veto on Casino
    data.VetoesOverridden.Should().Be(1);
    data.HatTrickTitles.Should().Be(1);
    data.DrafterDrafts.Single(r => r.DrafterId == DrafterId(1) && r.Appeared).EpisodeNumber.Should().Be(1);
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task QueryDatasetLoader_ShouldRunAgainstAnEmptyDatabaseAsync(bool includeAll)
  {
    await using var connection = await OpenAsync();

    var data = await QueryDatasetLoader.LoadAsync(connection, includeAll, TestContext.Current.CancellationToken);

    data.Picks.Should().BeEmpty();
    data.Credits.Should().BeEmpty();
    data.Vetoes.Should().BeEmpty();
  }

  [Theory]
  [InlineData(false, 8)]
  [InlineData(true, 9)]
  public async Task QueryDatasetLoader_ShouldMapEveryColumn_WhenFactsExistAsync(bool includeAll, int picks)
  {
    await SeedAsync();
    await using var connection = await OpenAsync();

    var data = await QueryDatasetLoader.LoadAsync(connection, includeAll, TestContext.Current.CancellationToken);

    data.Picks.Should().HaveCount(picks);
    data.Picks.Should().OnlyContain(p => p.DraftTitle.Length > 0 && p.SeriesName.Length > 0 && p.MediaTitle.Length > 0);
    data.Picks.Single(p => p.MediaTitle == "Dune").CommissionerOverridden.Should().BeTrue();
    data.Picks.Single(p => p.MediaTitle == "Dune").Landed.Should().BeFalse();
    data.Picks.Single(p => p.MediaTitle == "Alien").VetoStanding.Should().BeTrue();
    data.Picks.Where(p => p.DraftTitle == "One").Should().OnlyContain(p => p.EpisodeNumber == 1);
    data.Credits.Should().HaveCount(includeAll ? 9 : 8);
    data.Vetoes.Should().HaveCount(3);
    data.Vetoes.Should().Contain(v => v.IssuedByKind == CommunityKind);
    data.Vetoes.Single(v => v.IsOverridden).IssuedByIdValue.Should().Be(DrafterId(1));
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task TitleAppearanceLoader_ShouldRunAgainstAnEmptyDatabaseAsync(bool includeAll)
  {
    await using var connection = await OpenAsync();

    var rows = await TitleAppearanceLoader.LoadAsync(connection, includeAll, TestContext.Current.CancellationToken);

    rows.Should().BeEmpty();
  }

  [Fact]
  public async Task TitleAppearanceLoader_ShouldMapEveryColumn_WhenFactsExistAsync()
  {
    await SeedAsync();
    await using var connection = await OpenAsync();

    var rows = await TitleAppearanceLoader.LoadAsync(connection, includeAll: false, TestContext.Current.CancellationToken);

    var heat = rows.Where(r => r.MediaTitle == "Heat").OrderBy(r => r.PartIndex).ToList();
    heat.Should().HaveCount(2);
    heat[0].PartIndex.Should().Be(1);
    heat[0].TotalParts.Should().Be(2);
    heat[0].EpisodeNumber.Should().Be(1);
    heat[0].ReleasedOn.Should().Be("2026-01-01");
    heat[0].ReleaseRank.Should().Be(1);
    heat[1].ReleasedOn.Should().Be("2026-01-08");
    heat[1].ReleaseRank.Should().Be(2);

    rows.Should().NotContain(r => r.MediaTitle == "Dune", "a commissioner-removed pick is not an appearance");
    rows.Should().NotContain(r => r.MediaTitle == "Alien", "a pick vetoed with the veto standing is not an appearance");
  }
}
