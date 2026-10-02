using System.Text.RegularExpressions;

using WikiExportPermission = ScreenDrafts.Modules.Drafts.Features.DraftsAuth.Permissions;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// HTTP-level coverage for <c>POST /wiki-exports/drafts</c> and <c>POST /wiki-exports/drafters</c>:
/// authentication (401), authorization (403), validation (400), not-found (404) and the 200 body.
/// Requests authenticate through <see cref="TestAuthentication"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Usage",
  "CA2234:Pass system uri objects instead of strings",
  Justification = "Relative route strings are the contract under test."
)]
public sealed partial class WikiExportEndpointTests(DraftsIntegrationTestWebAppFactory factory)
  : WikiExportIntegrationTest(factory)
{
  private const string DraftsRoute = "/wiki-exports/drafts";
  private const string DraftersRoute = "/wiki-exports/drafters";
  private const string Permission = WikiExportPermission.WikiExport;

  [GeneratedRegex(@"^screendrafts-wiki-(drafts|drafters)-\d{8}\.txt$")]
  private static partial Regex FileNamePattern();

  public static TheoryData<string, string> Endpoints =>
    new()
    {
      { DraftsRoute, "draftPublicIds" },
      { DraftersRoute, "drafterPublicIds" },
    };

  private static string ValidId(string route, int n) =>
    $"{(route == DraftsRoute ? PublicIdPrefixes.Draft : PublicIdPrefixes.Drafter)}_{n:D15}";

  private static Dictionary<string, object?> Body(string property, IEnumerable<string>? ids) =>
    new() { [property] = ids?.ToList() };

  private Task<HttpResponseMessage> PostAsync(string route, string property, IEnumerable<string>? ids) =>
    HttpClient.PostAsJsonAsync(route, Body(property, ids), TestContext.Current.CancellationToken);

  // -------------------------------------------------------------------------
  // 401 / 403
  // -------------------------------------------------------------------------

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnUnauthorized_WhenNoTokenIsProvidedAsync(string route, string property)
  {
    var response = await PostAsync(route, property, [ValidId(route, 1)]);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnForbidden_WhenUserLacksTheWikiExportPermissionAsync(
    string route,
    string property
  )
  {
    HttpClient.AuthenticateWith("drafts:read", "drafts:update");

    var response = await PostAsync(route, property, [ValidId(route, 1)]);

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnForbidden_WhenUserHasNoPermissionsAtAllAsync(
    string route,
    string property
  )
  {
    HttpClient.AuthenticateWith();

    var response = await PostAsync(route, property, [ValidId(route, 1)]);

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  // -------------------------------------------------------------------------
  // 200
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PostDrafts_ShouldReturnOkWithAnExportWikiResponse_WhenUserHasThePermissionAsync()
  {
    var draft = await Seed.DraftAsync("Endpoint Draft", episodeNumber: 7);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 5, 1));
    var alice = await Seed.ParticipantAsync(part, await Seed.DrafterAsync("Alice Anderson"));
    await Seed.PickAsync(part, alice, await Seed.MovieAsync("Endpoint Film"), 7, 1);
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(DraftsRoute, "draftPublicIds", [draft.PublicId]);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadFromJsonAsync<ExportWikiResponse>(
      TestContext.Current.CancellationToken
    );
    body.Should().NotBeNull();
    body.PageCount.Should().Be(1);
    body.Content.Should().StartWith("=== Page: Endpoint Draft ===\n\n{{Episodes");
    body.Content.Should().Contain("7. [[Endpoint Film]] by [[Alice Anderson]]");
    body.FileName.Should().MatchRegex(FileNamePattern().ToString());
    body.FileName.Should().StartWith("screendrafts-wiki-drafts-");
  }

  [Fact]
  public async Task PostDrafters_ShouldReturnOkWithAnExportWikiResponse_WhenUserHasThePermissionAsync()
  {
    var alice = await Seed.DrafterAsync("Alice Anderson");
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(DraftersRoute, "drafterPublicIds", [alice.PublicId]);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadFromJsonAsync<ExportWikiResponse>(
      TestContext.Current.CancellationToken
    );
    body.Should().NotBeNull();
    body.PageCount.Should().Be(1);
    body.Content.Should().StartWith("=== Page: Alice Anderson ===\n\n{{Drafter");
    body.FileName.Should().MatchRegex(FileNamePattern().ToString());
    body.FileName.Should().StartWith("screendrafts-wiki-drafters-");
  }

  // -------------------------------------------------------------------------
  // 400
  // -------------------------------------------------------------------------

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnBadRequest_WhenTheListIsEmptyAsync(string route, string property)
  {
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(route, property, []);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnBadRequest_When51IdsAreSentAsync(string route, string property)
  {
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(
      route,
      property,
      Enumerable.Range(1, 51).Select(n => ValidId(route, n))
    );

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldNotReturnBadRequest_WhenExactly50IdsAreSentAsync(
    string route,
    string property
  )
  {
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(
      route,
      property,
      Enumerable.Range(1, 50).Select(n => ValidId(route, n))
    );

    // Well-formed but unknown, so the handler answers 404 rather than validation answering 400.
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnBadRequest_WhenAnIdIsMalformedOrHasTheWrongPrefixAsync(
    string route,
    string property
  )
  {
    HttpClient.AuthenticateWith(Permission);
    var wrongPrefix = route == DraftsRoute ? "dr_000000000000001" : "d_000000000000001";

    var malformed = await PostAsync(route, property, [ValidId(route, 1), "nope"]);
    var prefixed = await PostAsync(route, property, [wrongPrefix]);

    malformed.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    prefixed.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnBadRequest_WhenTheListIsExplicitlyNullAsync(
    string route,
    string property
  )
  {
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(route, property, null);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  // -------------------------------------------------------------------------
  // 404
  // -------------------------------------------------------------------------

  [Theory]
  [MemberData(nameof(Endpoints))]
  public async Task Post_ShouldReturnNotFound_WhenNothingRequestedExistsAsync(
    string route,
    string property
  )
  {
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(route, property, [ValidId(route, 42)]);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task PostDrafts_ShouldReturnNotFound_WhenEveryRequestedDraftIsPatreonOnlyAsync()
  {
    var patreon = await Seed.DraftAsync("Patreon Only Draft");
    await Seed.PartAsync(patreon, 1, patreon: Date(2024, 1, 2));
    HttpClient.AuthenticateWith(Permission);

    var response = await PostAsync(DraftsRoute, "draftPublicIds", [patreon.PublicId]);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }
}
