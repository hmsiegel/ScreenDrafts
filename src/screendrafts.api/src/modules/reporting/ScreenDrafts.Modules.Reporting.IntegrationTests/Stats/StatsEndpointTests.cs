namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Stats;

/// <summary>
/// HTTP-level coverage of the stats endpoints: anonymous access, the 401 on the signed-in ones, the
/// <c>includeAll</c> toggle that only Patreon members get, and the 400 / 404 problem responses.
/// Requests authenticate through <see cref="TestAuthentication"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Usage",
  "CA2234:Pass system uri objects instead of strings",
  Justification = "Relative route strings are the contract under test."
)]
public sealed class StatsEndpointTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private const string PatreonPermission = "drafts:read-patreon";
  private static readonly DateOnly _jan1 = new(2026, 1, 1);

  /// <summary>One canonical draft and one policy 1 (Patreon / Speed) draft, each with a repeated title.</summary>
  private async Task SeedAsync()
  {
    var seed = new StatsSeeder(DbContext);
    seed.Draft("Canon").Part(episode: 1, mainFeed: _jan1).Pick("Heat", 1, 1);
    seed.Draft("Speed", type: "Speed", policy: 1)
      .Part(episode: 2, mainFeed: _jan1.AddDays(1))
      .Pick("Heat", 1, 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);
  }

  private static StringContent QueryBody(string metric, string groupBy, bool includeAll = false) =>
    new(
      $$"""{"metric":"{{metric}}","groupBy":"{{groupBy}}","includeAll":{{(includeAll ? "true" : "false")}}}""",
      System.Text.Encoding.UTF8,
      "application/json"
    );

  private async Task<HttpResponseMessage> PostQueryAsync(
    string metric,
    string groupBy,
    bool includeAll = false
  )
  {
    using var body = QueryBody(metric, groupBy, includeAll);
    return await HttpClient.PostAsync("/stats/query", body, TestContext.Current.CancellationToken);
  }

  // -------------------------------------------------------------------------
  // Record Book
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetRecordBook_ShouldReturnOk_WhenCalledAnonymouslyAsync()
  {
    var response = await HttpClient.GetAsync(
      "/stats/record-book",
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadFromJsonAsync<GetRecordBookResponse>(
      TestContext.Current.CancellationToken
    );
    body!.Totals.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetRecordBook_ShouldIgnoreIncludeAll_WhenTheCallerIsNotAPatreonMemberAsync()
  {
    await SeedAsync();

    var anonymous = await HttpClient.GetFromJsonAsync<GetRecordBookResponse>(
      "/stats/record-book?includeAll=true",
      TestContext.Current.CancellationToken
    );

    HttpClient.AuthenticateWith("stats:read");
    var signedIn = await HttpClient.GetFromJsonAsync<GetRecordBookResponse>(
      "/stats/record-book?includeAll=true",
      TestContext.Current.CancellationToken
    );

    anonymous!.IncludesNonCanonical.Should().BeFalse();
    signedIn!.IncludesNonCanonical.Should().BeFalse();
    anonymous.Totals.Single(t => t.Code == "drafts").Value.Should().Be(1);
  }

  [Fact]
  public async Task GetRecordBook_ShouldHonorIncludeAll_WhenTheCallerHasThePatreonPermissionAsync()
  {
    await SeedAsync();
    HttpClient.AuthenticateWith(PatreonPermission);

    var body = await HttpClient.GetFromJsonAsync<GetRecordBookResponse>(
      "/stats/record-book?includeAll=true",
      TestContext.Current.CancellationToken
    );

    body!.IncludesNonCanonical.Should().BeTrue();
    body.Totals.Single(t => t.Code == "drafts").Value.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Titles
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData("marquee-of-fame")]
  [InlineData("hat-trick")]
  [InlineData("grand-slam")]
  [InlineData("high-five")]
  [InlineData("6-drafts")]
  [InlineData("10-drafts")]
  public async Task GetTitles_ShouldReturnOk_WhenCalledAnonymouslyAsync(string level)
  {
    var response = await HttpClient.GetAsync(
      $"/stats/titles/{level}",
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadFromJsonAsync<GetTitleHonorificsResponse>(
      TestContext.Current.CancellationToken
    );
    body!.Level.Should().Be(level);
    body.Counts.Should().HaveCount(9);
  }

  [Fact]
  public async Task GetTitles_ShouldReturnNotFound_WhenTheLevelIsUnknownAsync()
  {
    var response = await HttpClient.GetAsync(
      "/stats/titles/six-pack",
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Theory]
  [InlineData("sort=random")]
  [InlineData("page=0")]
  [InlineData("pageSize=0")]
  [InlineData("pageSize=101")]
  public async Task GetTitles_ShouldReturnBadRequest_WhenAParameterIsInvalidAsync(string query)
  {
    var response = await HttpClient.GetAsync(
      $"/stats/titles/hat-trick?{query}",
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetTitles_ShouldReturnBadRequest_WhenSearchIsTooLongAsync()
  {
    var search = new string('x', 101);

    var response = await HttpClient.GetAsync(
      $"/stats/titles/hat-trick?search={search}",
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetTitles_ShouldHonorIncludeAll_OnlyForPatreonMembersAsync()
  {
    await SeedAsync();

    var anonymous = await HttpClient.GetFromJsonAsync<GetTitleHonorificsResponse>(
      "/stats/titles/marquee-of-fame?includeAll=true",
      TestContext.Current.CancellationToken
    );

    HttpClient.AuthenticateWith(PatreonPermission);
    var patreon = await HttpClient.GetFromJsonAsync<GetTitleHonorificsResponse>(
      "/stats/titles/marquee-of-fame?includeAll=true",
      TestContext.Current.CancellationToken
    );

    anonymous!.IncludesNonCanonical.Should().BeFalse();
    anonymous.Titles.Should().BeEmpty();
    patreon!.IncludesNonCanonical.Should().BeTrue();
    patreon.Titles.Should().ContainSingle().Which.Title.Should().Be("Heat");
  }

  // -------------------------------------------------------------------------
  // POST /stats/query
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PostQuery_ShouldReturnUnauthorized_WhenNoTokenIsProvidedAsync()
  {
    var response = await PostQueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task PostQuery_ShouldReturnOk_WhenSignedInWithoutAnySpecialPermissionAsync()
  {
    HttpClient.AuthenticateWith();

    var response = await PostQueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task PostQuery_ShouldReturnBadRequest_WhenTheMetricAndGroupByDoNotPairAsync()
  {
    HttpClient.AuthenticateWith();

    var response = await PostQueryAsync(StatsMetrics.CopaceticDrafts, StatsGroupBys.Title);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task PostQuery_ShouldReturnBadRequest_WhenTheMetricIsUnknownAsync()
  {
    HttpClient.AuthenticateWith();

    var response = await PostQueryAsync("nonsense", StatsGroupBys.Drafter);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task PostQuery_ShouldHonorIncludeAll_OnlyForPatreonMembersAsync()
  {
    await SeedAsync();

    HttpClient.AuthenticateWith("stats:read");
    var plain = await (
      await PostQueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Series, includeAll: true)
    ).Content.ReadFromJsonAsync<QueryStatsResponse>(TestContext.Current.CancellationToken);

    HttpClient.AuthenticateWith(PatreonPermission);
    var patreon = await (
      await PostQueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Series, includeAll: true)
    ).Content.ReadFromJsonAsync<QueryStatsResponse>(TestContext.Current.CancellationToken);

    plain!.IncludesNonCanonical.Should().BeFalse();
    plain.Rows.Should().ContainSingle();
    patreon!.IncludesNonCanonical.Should().BeTrue();
    patreon.Rows.Should().HaveCount(1, "both drafts share the default series name");
    patreon.Rows[0].Value.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // GET /stats/query/options
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetOptions_ShouldReturnUnauthorized_WhenNoTokenIsProvidedAsync()
  {
    var response = await HttpClient.GetAsync(
      "/stats/query/options",
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetOptions_ShouldReportCanIncludeAllFromThePatreonPermissionAsync()
  {
    await SeedAsync();

    HttpClient.AuthenticateWith();
    var plain = await HttpClient.GetFromJsonAsync<StatsQueryOptionsResponse>(
      "/stats/query/options",
      TestContext.Current.CancellationToken
    );

    HttpClient.AuthenticateWith(PatreonPermission);
    var patreon = await HttpClient.GetFromJsonAsync<StatsQueryOptionsResponse>(
      "/stats/query/options",
      TestContext.Current.CancellationToken
    );

    plain!.CanIncludeAll.Should().BeFalse();
    plain.DraftTypes.Should().Equal("Standard");
    patreon!.CanIncludeAll.Should().BeTrue();
    patreon.DraftTypes.Should().Equal("Speed", "Standard");
  }
}
