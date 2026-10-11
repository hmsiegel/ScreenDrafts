using System.Net;
using System.Net.Http.Json;

using ScreenDrafts.Common.IntegrationTests.Abstractions;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations;

/// <summary>
/// HTTP-level coverage for the invitation endpoints: authentication (401), authorization,
/// validation (400), not-found (404), conflict (409) and the happy paths. Requests
/// authenticate through <see cref="TestAuthentication"/> as <see cref="InvitationIntegrationTest.HttpUserPublicId"/>.
/// </summary>
public sealed class InvitationEndpointTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : InvitationIntegrationTest(factory)
{
  private const string Permission = "guest-drafts:add-participant";
  private static readonly TimeSpan Cooldown = TimeSpan.FromHours(24);

  private static Uri InvitationsUri(string draftPublicId) =>
    new($"/guest-drafts/{draftPublicId}/invitations", UriKind.Relative);

  private static Uri ResendUri(string draftPublicId, string invitationPublicId) =>
    new($"/guest-drafts/{draftPublicId}/invitations/{invitationPublicId}/resend", UriKind.Relative);

  /// <summary>Makes the authenticated HTTP identity resolve to <paramref name="owner"/>'s account.</summary>
  // The UI always sends a JSON body on resend, and the endpoint answers 415 without one.
  private Task<HttpResponseMessage> ResendAsync(string draftPublicId, string invitationPublicId) =>
    HttpClient.PostAsJsonAsync(ResendUri(draftPublicId, invitationPublicId), new { }, Ct);

  private static async Task AssertOwnerOnlyErrorAsync(HttpResponseMessage response)
  {
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    (await response.Content.ReadAsStringAsync(Ct))
      .Should()
      .Contain(DraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  private async Task ActAsAsync(TestUser owner)
  {
    var user = await FakeUsersApi.GetUserByPublicId(owner.UserPublicId, Ct);
    FakeUsersApi.RegisterUser(user!.UserId, HttpUserPublicId, email: user.Email);
    HttpClient.AuthenticateWith(Permission);
  }

  private async Task<string> CreateDraftOwnedByHttpUserAsync()
  {
    var owner = await CreateUserAsync();
    await ActAsAsync(owner);
    return await CreateGuestDraftAsync(owner.UserPublicId);
  }

  private async Task InviteManyAsync(string draftPublicId, int count)
  {
    for (var i = 0; i < count; i++)
    {
      (await InviteAsync(draftPublicId, HttpUserPublicId, $"seed{i}@example.com", $"Seed {i}"))
        .IsSuccess.Should()
        .BeTrue();
    }
  }

  // ── POST /guest-drafts/{id}/invitations ───────────────────────────────────

  [Fact]
  public async Task Invite_ShouldReturnUnauthorized_WhenNoTokenIsProvidedAsync()
  {
    var draftPublicId = await CreateGuestDraftAsync((await CreateUserAsync()).UserPublicId);

    var response = await PostInviteAsync(draftPublicId, "guest@example.com", "Guest");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Invite_ShouldReturnForbidden_WhenTheUserLacksThePermissionAsync()
  {
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    HttpClient.AuthenticateWith("guest-drafts:read");

    var response = await PostInviteAsync(draftPublicId, "guest@example.com", "Guest");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task Invite_ShouldReturnOkWithAPublicId_WhenTheOwnerInvitesAsync()
  {
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();

    var response = await PostInviteAsync(draftPublicId, "Guest@Example.com", "Guest Person");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadFromJsonAsync<InviteGuestDrafterResponse>(Ct);
    body!.PublicId.Should().StartWith("gdi_");
    var stored = (await LoadInvitationsAsync(draftPublicId)).Should().ContainSingle().Subject;
    stored.PublicId.Should().Be(body.PublicId);
    stored.Email.Should().Be("guest@example.com");
  }

  [Fact]
  public async Task Invite_ShouldReturnBadRequestWithTheOwnerOnlyError_WhenTheCallerIsNotTheOwnerAsync()
  {
    var draftPublicId = await CreateGuestDraftAsync((await CreateUserAsync()).UserPublicId);
    await CreateHttpUserAsync();
    HttpClient.AuthenticateWith(Permission);

    var response = await PostInviteAsync(draftPublicId, "guest@example.com", "Guest");

    // Owner-only failures are a Problem error, which every GuestDrafts endpoint maps to 400.
    await AssertOwnerOnlyErrorAsync(response);
    (await LoadInvitationsAsync(draftPublicId)).Should().BeEmpty();
  }

  [Fact]
  public async Task Invite_ShouldReturnNotFound_WhenTheDraftDoesNotExistAsync()
  {
    await ActAsAsync(await CreateUserAsync());

    var response = await PostInviteAsync("gd_doesnotexist0000", "guest@example.com", "Guest");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task Invite_ShouldReturnBadRequest_WhenTheDraftHasAlreadyStartedAsync()
  {
    var (draftPublicId, ownerPublicId, _) = await CreateInProgressStandardGuestDraftAsync();
    var owner = await FakeUsersApi.GetUserByPublicId(ownerPublicId, Ct);
    FakeUsersApi.RegisterUser(owner!.UserId, HttpUserPublicId);
    HttpClient.AuthenticateWith(Permission);

    var response = await PostInviteAsync(draftPublicId, "guest@example.com", "Guest");

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Invite_ShouldReturnConflict_WhenTheEmailBelongsToARegisteredUserAsync()
  {
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    FakeUsersApi.RegisterUser(
      Guid.NewGuid(),
      InvitationTestData.NewUserPublicId(),
      email: "member@example.com"
    );

    var response = await PostInviteAsync(draftPublicId, "Member@Example.com", "Existing Member");

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    (await LoadInvitationsAsync(draftPublicId)).Should().BeEmpty();
  }

  [Fact]
  public async Task Invite_ShouldReturnConflict_WhenTheEmailWasAlreadyInvitedAsync()
  {
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    (await PostInviteAsync(draftPublicId, "dup@example.com", "Dup")).StatusCode.Should().Be(HttpStatusCode.OK);

    var response = await PostInviteAsync(draftPublicId, "DUP@example.com", "Dup Again");

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    (await LoadInvitationsAsync(draftPublicId)).Should().ContainSingle();
  }

  [Fact]
  public async Task Invite_ShouldReturnConflict_WhenTheEleventhInvitationIsSentAsync()
  {
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    await InviteManyAsync(draftPublicId, 10);

    var response = await PostInviteAsync(draftPublicId, "eleventh@example.com", "Eleventh");

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    (await LoadInvitationsAsync(draftPublicId)).Should().HaveCount(10);
  }

  [Theory]
  [InlineData("not-an-email", "Guest")]
  [InlineData("", "Guest")]
  [InlineData("guest@example.com", "")]
  public async Task Invite_ShouldReturnBadRequest_WhenTheBodyIsInvalidAsync(string email, string name)
  {
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();

    var response = await PostInviteAsync(draftPublicId, email, name);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    (await LoadInvitationsAsync(draftPublicId)).Should().BeEmpty();
  }

  // ── GET /guest-drafts/{id}/invitations ────────────────────────────────────

  [Fact]
  public async Task List_ShouldReturnUnauthorized_WhenNoTokenIsProvidedAsync()
  {
    var response = await HttpClient.GetAsync(InvitationsUri("gd_abcdefghijklmno"), Ct);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task List_ShouldReturnTheCreatedInvitationsToTheOwnerAsync()
  {
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    await InviteManyAsync(draftPublicId, 2);

    var response = await HttpClient.GetAsync(InvitationsUri(draftPublicId), Ct);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadFromJsonAsync<ListGuestDraftInvitationsResponse>(Ct);
    body!.Invitations.Select(i => i.Email).Should().Equal("seed0@example.com", "seed1@example.com");
    body.Invitations.Should().OnlyContain(i => i.Status == "Pending");
  }

  [Fact]
  public async Task List_ShouldReturnBadRequestWithTheOwnerOnlyError_WhenTheCallerIsNotTheOwnerAsync()
  {
    var draftPublicId = await CreateGuestDraftAsync((await CreateUserAsync()).UserPublicId);
    await CreateHttpUserAsync();
    HttpClient.AuthenticateWith(Permission);

    var response = await HttpClient.GetAsync(InvitationsUri(draftPublicId), Ct);

    await AssertOwnerOnlyErrorAsync(response);
  }

  // ── POST /guest-drafts/{id}/invitations/{invitationId}/resend ─────────────

  [Fact]
  public async Task Resend_ShouldReturnConflict_ImmediatelyAfterTheInviteAsync()
  {
    Clock.Pin(FixedNow);
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    var invited = await InviteAsync(draftPublicId, HttpUserPublicId);

    var response = await ResendAsync(draftPublicId, invited.Value.PublicId);

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task Resend_ShouldReturnNoContent_OnceTheClockHasAdvancedPastTwentyFourHoursAsync()
  {
    Clock.Pin(FixedNow);
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    var invited = await InviteAsync(draftPublicId, HttpUserPublicId);
    Clock.Advance(Cooldown + TimeSpan.FromMinutes(1));

    var response = await ResendAsync(draftPublicId, invited.Value.PublicId);

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    (await LoadInvitationsAsync(draftPublicId))
      .Single()
      .LastSentOnUtc.Should()
      .Be(FixedNow + Cooldown + TimeSpan.FromMinutes(1));
  }

  [Fact]
  public async Task Resend_ShouldReturnNotFound_WhenTheInvitationBelongsToAnotherDraftAsync()
  {
    Clock.Pin(FixedNow);
    var draftPublicId = await CreateDraftOwnedByHttpUserAsync();
    var otherDraftPublicId = await CreateGuestDraftAsync(HttpUserPublicId);
    var foreign = await InviteAsync(otherDraftPublicId, HttpUserPublicId, "foreign@example.com", "Foreign");
    Clock.Advance(Cooldown + TimeSpan.FromMinutes(1));

    var response = await ResendAsync(draftPublicId, foreign.Value.PublicId);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task Resend_ShouldReturnUnauthorized_WhenNoTokenIsProvidedAsync()
  {
    var response = await ResendAsync("gd_abcdefghijklmno", "gdi_abcdefghijklmno");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}
