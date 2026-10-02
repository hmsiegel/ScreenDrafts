namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

/// <summary>
/// HTTP-level coverage for <c>PUT /reporting/spotlights/{publicId}</c>: real pipeline (routing,
/// authorization, validation, unit of work), real Postgres and real Redis. Requests authenticate
/// through <see cref="TestAuthentication"/> so no Keycloak or foreign schema is involved.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Usage",
  "CA2234:Pass system uri objects instead of strings",
  Justification = "Relative route strings are the contract under test."
)]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1054:URI parameters should not be strings",
  Justification = "The request body carries the URL as a string; malformed values are the point."
)]
public sealed class UpdateSpotlightTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private const string SpotlightRoute = "/spotlight";
  private const string ManagePermission = ReportingAuth.Permissions.SpotlightManage;

  private static string NewSpotlightPublicId() =>
    $"{PublicIdPrefixes.Spotlight}_{Faker.Random.AlphaNumeric(15)}";

  private static string UpdateRoute(string publicId) => $"/reporting/spotlights/{publicId}";

  private static string ActivateRoute(string publicId) =>
    $"/reporting/spotlights/{publicId}/activate";

  private static object Body(string description, string? spotifyUrl = null) =>
    new { spotlightDescription = description, spotifyUrl };

  private async Task<DraftSpotlight> SeedSpotlightAsync(
    string description = "Original description",
    string? spotifyUrl = null,
    bool active = false
  )
  {
    var draftPublicId = $"{PublicIdPrefixes.Draft}_{Faker.Random.AlphaNumeric(15)}";

    DbContext.DraftSummaries.Add(
      DraftSummary.Create(
        draftId: Guid.NewGuid(),
        draftPublicId: draftPublicId,
        draftPartPublicId: $"{PublicIdPrefixes.DraftPart}_{Faker.Random.AlphaNumeric(15)}",
        title: "Spotlight Draft",
        draftType: "Standard",
        partIndex: 1,
        totalParts: 1,
        totalPicks: 7,
        isPatreon: false,
        episodeNumber: 12,
        isComplete: true,
        completedAtUtc: DateTime.UtcNow.AddDays(-1),
        createdAtUtc: DateTime.UtcNow.AddDays(-2)
      )
    );

    var spotlight = DraftSpotlight.Create(
      NewSpotlightPublicId(),
      draftPublicId,
      description,
      spotifyUrl is null ? null : new Uri(spotifyUrl)
    );

    if (active)
    {
      spotlight.Activate();
      spotlight.Pin();
    }

    DbContext.DraftSpotlights.Add(spotlight);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    return spotlight;
  }

  private Task<DraftSpotlight> ReloadAsync(string publicId) =>
    DbContext
      .DraftSpotlights.AsNoTracking()
      .SingleAsync(s => s.PublicId == publicId, TestContext.Current.CancellationToken);

  private async Task<GetActiveSpotlightResponse> GetActiveAsync()
  {
    var response = await HttpClient.GetAsync(SpotlightRoute, TestContext.Current.CancellationToken);
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var body = await response.Content.ReadFromJsonAsync<GetActiveSpotlightResponse>(
      TestContext.Current.CancellationToken
    );
    return body!;
  }

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Put_ShouldReturnNoContentAndPersistTheChanges_WhenRequestIsValidAsync()
  {
    var spotlight = await SeedSpotlightAsync();
    HttpClient.AuthenticateWith(ManagePermission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("Brand new copy", "https://open.spotify.com/episode/fresh"),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    var reloaded = await ReloadAsync(spotlight.PublicId);
    reloaded.SpotlightDescription.Should().Be("Brand new copy");
    reloaded.SpotifyUrl.Should().Be(new Uri("https://open.spotify.com/episode/fresh"));
  }

  [Fact]
  public async Task Put_ShouldClearTheSpotifyUrl_WhenUrlIsOmittedAsync()
  {
    var spotlight = await SeedSpotlightAsync(spotifyUrl: "https://open.spotify.com/episode/old");
    HttpClient.AuthenticateWith(ManagePermission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("Same copy, no link"),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    (await ReloadAsync(spotlight.PublicId)).SpotifyUrl.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Put_ShouldReturnNotFound_WhenSpotlightDoesNotExistAsync()
  {
    HttpClient.AuthenticateWith(ManagePermission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(NewSpotlightPublicId()),
      Body("Anything"),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  // -------------------------------------------------------------------------
  // Validation
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Put_ShouldReturnBadRequest_WhenDescriptionIsEmptyAsync()
  {
    var spotlight = await SeedSpotlightAsync();
    HttpClient.AuthenticateWith(ManagePermission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body(string.Empty),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    (await ReloadAsync(spotlight.PublicId)).SpotlightDescription.Should().Be("Original description");
  }

  [Fact]
  public async Task Put_ShouldReturnBadRequest_WhenDescriptionIsLongerThan1000CharactersAsync()
  {
    var spotlight = await SeedSpotlightAsync();
    HttpClient.AuthenticateWith(ManagePermission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body(new string('a', 1001)),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Put_ShouldReturnNoContent_WhenDescriptionIsExactly1000CharactersAsync()
  {
    var spotlight = await SeedSpotlightAsync();
    HttpClient.AuthenticateWith(ManagePermission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body(new string('a', 1000)),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
  }

  [Theory]
  [InlineData("not a url")]
  [InlineData("open.spotify.com/episode/abc")]
  [InlineData("/episode/abc")]
  public async Task Put_ShouldReturnBadRequest_WhenSpotifyUrlIsNotAnAbsoluteUrlAsync(string url)
  {
    var spotlight = await SeedSpotlightAsync(spotifyUrl: "https://open.spotify.com/episode/old");
    HttpClient.AuthenticateWith(ManagePermission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("Valid description", url),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    (await ReloadAsync(spotlight.PublicId))
      .SpotifyUrl.Should()
      .Be(new Uri("https://open.spotify.com/episode/old"));
  }

  // -------------------------------------------------------------------------
  // Authentication / authorization
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Put_ShouldReturnUnauthorized_WhenNoTokenIsProvidedAsync()
  {
    var spotlight = await SeedSpotlightAsync();

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("Should not land"),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    (await ReloadAsync(spotlight.PublicId)).SpotlightDescription.Should().Be("Original description");
  }

  [Theory]
  [InlineData(ReportingAuth.Permissions.SpotlightUpdate)]
  [InlineData(ReportingAuth.Permissions.SpotlightRead)]
  [InlineData("drafts:read")]
  public async Task Put_ShouldReturnForbidden_WhenUserLacksSpotlightManageAsync(string permission)
  {
    var spotlight = await SeedSpotlightAsync();
    HttpClient.AuthenticateWith(permission);

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("Should not land"),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    (await ReloadAsync(spotlight.PublicId)).SpotlightDescription.Should().Be("Original description");
  }

  [Fact]
  public async Task Put_ShouldReturnForbidden_WhenUserHasNoPermissionsAtAllAsync()
  {
    var spotlight = await SeedSpotlightAsync();
    HttpClient.AuthenticateWith();

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("Should not land"),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  // -------------------------------------------------------------------------
  // Cache coherence with the active-spotlight read endpoint
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Put_ShouldBeVisibleThroughTheActiveSpotlightEndpoint_WhenSpotlightIsActiveAsync()
  {
    var spotlight = await SeedSpotlightAsync(description: "Stale copy", active: true);
    HttpClient.AuthenticateWith(ManagePermission);

    // Prime the cache with the pre-update value.
    (await GetActiveAsync()).SpotlightDescription.Should().Be("Stale copy");
    (await GetService<IDistributedCache>().GetAsync("reporting:spotlight:active", TestContext.Current.CancellationToken))
      .Should()
      .NotBeNull("the read above should have cached the active spotlight");

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("Fresh copy", "https://open.spotify.com/episode/fresh"),
      TestContext.Current.CancellationToken
    );
    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var active = await GetActiveAsync();
    active.SpotlightDescription.Should().Be("Fresh copy");
    active.SpotifyUrl.Should().Be("https://open.spotify.com/episode/fresh");
  }

  [Fact]
  public async Task Put_ShouldLeaveTheCachedActiveSpotlightAlone_WhenTheUpdatedSpotlightIsInactiveAsync()
  {
    await SeedSpotlightAsync(description: "Active copy", active: true);
    var inactive = await SeedSpotlightAsync(description: "Inactive copy");
    HttpClient.AuthenticateWith(ManagePermission);
    (await GetActiveAsync()).SpotlightDescription.Should().Be("Active copy");

    var response = await HttpClient.PutAsJsonAsync(
      UpdateRoute(inactive.PublicId),
      Body("Inactive copy, edited"),
      TestContext.Current.CancellationToken
    );

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    (await GetService<IDistributedCache>().GetAsync("reporting:spotlight:active", TestContext.Current.CancellationToken))
      .Should()
      .NotBeNull("editing an inactive spotlight must not evict the active spotlight's cache entry");
    (await GetActiveAsync()).SpotlightDescription.Should().Be("Active copy");
  }

  // -------------------------------------------------------------------------
  // Route regression: Update and Activate share a prefix and must not collide
  // -------------------------------------------------------------------------

  [Fact]
  public void Routes_ShouldRegisterUpdateAndActivateAsDistinctPutEndpoints()
  {
    var putRoutes = Factory
      .Services.GetRequiredService<EndpointDataSource>()
      .Endpoints.OfType<RouteEndpoint>()
      .Where(e =>
        e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(HttpMethods.Put) == true
      )
      .Select(e => e.RoutePattern.RawText)
      .ToList();

    putRoutes.Count(r => r == "/reporting/spotlights/{publicId}").Should().Be(1);
    putRoutes.Count(r => r == "/reporting/spotlights/{publicId}/activate").Should().Be(1);
  }

  [Fact]
  public async Task Put_ShouldRouteUpdateAndActivateIndependently_InTheSameAppAsync()
  {
    var spotlight = await SeedSpotlightAsync(description: "Before");
    HttpClient.AuthenticateWith(ManagePermission);

    // The UI sends an empty JSON object for activate; FastEndpoints rejects a bodiless PUT (415).
    var activate = await HttpClient.PutAsJsonAsync(
      ActivateRoute(spotlight.PublicId),
      new { },
      TestContext.Current.CancellationToken
    );
    var update = await HttpClient.PutAsJsonAsync(
      UpdateRoute(spotlight.PublicId),
      Body("After"),
      TestContext.Current.CancellationToken
    );

    activate.StatusCode.Should().Be(HttpStatusCode.NoContent);
    update.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var reloaded = await ReloadAsync(spotlight.PublicId);
    reloaded.IsActive.Should().BeTrue("the activate route must still activate");
    reloaded.SpotlightDescription.Should().Be("After", "the update route must still update");
  }
}
