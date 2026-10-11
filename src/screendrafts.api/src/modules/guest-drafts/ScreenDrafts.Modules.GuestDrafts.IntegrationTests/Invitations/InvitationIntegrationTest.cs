using System.Net.Http.Json;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations;

/// <summary>
/// Shared helpers for the database-backed invitation tests. The inherited DbContext and
/// Sender share one DI scope, so reads that must prove a row really reached PostgreSQL go
/// through <see cref="InFreshScopeAsync{T}"/> instead.
/// </summary>
public abstract class InvitationIntegrationTest(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  /// <summary>The public id every <c>Authorization: Test ...</c> request carries.</summary>
  protected const string HttpUserPublicId = "u_integrationtest0001";

  protected static readonly DateTime FixedNow = new(2026, 10, 11, 12, 0, 0, DateTimeKind.Utc);

  protected static CancellationToken Ct => TestContext.Current.CancellationToken;

  protected async Task<T> InFreshScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
  {
    ArgumentNullException.ThrowIfNull(action);

    using var scope = Factory.Services.CreateScope();
    return await action(scope.ServiceProvider);
  }

  /// <summary>Registers the user that HTTP requests authenticate as, plus its GuestDrafter.</summary>
  protected async Task<TestUser> CreateHttpUserAsync()
  {
    var userId = Guid.NewGuid();
    FakeUsersApi.RegisterUser(userId, HttpUserPublicId, email: "http.user@example.com");

    var drafter = await Sender.Send(
      new CreateDrafterCommand
      {
        UserId = userId,
        FirstName = "Http",
        LastName = "User",
      },
      Ct
    );
    drafter.IsSuccess.Should().BeTrue();

    var drafterId = (await DbContext.Drafters.FirstAsync(d => d.PublicId == drafter.Value, Ct)).Id.Value;
    return new TestUser(HttpUserPublicId, drafter.Value, drafterId);
  }

  protected async Task<Draft> LoadDraftAsync(string draftPublicId) =>
    await DbContext.Drafts.AsNoTracking().FirstAsync(d => d.PublicId == draftPublicId, Ct);

  internal Task<Result<InviteGuestDrafterResponse>> InviteAsync(
    string draftPublicId,
    string callerUserPublicId,
    string email = "guest@example.com",
    string name = "Guest Person"
  ) =>
    Sender.Send(
      new InviteGuestDrafterCommand
      {
        GuestDraftPublicId = draftPublicId,
        CallerUserPublicId = callerUserPublicId,
        Email = email,
        Name = name,
      },
      Ct
    );

  /// <summary>Creates and persists an invitation straight through the repository.</summary>
  protected async Task<Invitation> AddInvitationAsync(
    Draft draft,
    string email = "guest@example.com",
    string name = "Guest Person",
    DateTime? sentAt = null
  )
  {
    ArgumentNullException.ThrowIfNull(draft);

    var invitation = Invitation
      .Create(
        publicId: GetService<IPublicIdGenerator>().GeneratePublicId(PublicIdPrefixes.GuestDraftInvitation),
        draftId: draft.Id,
        draftPublicId: draft.PublicId,
        email: email,
        name: name,
        invitedByUserId: draft.OwnerUserId,
        utcNow: sentAt ?? FixedNow
      )
      .Value;

    GetService<IInvitationRepository>().Add(invitation);
    await DbContext.SaveChangesAsync(Ct);
    return invitation;
  }

  protected async Task<IReadOnlyList<Invitation>> LoadInvitationsAsync(string draftPublicId)
  {
    var draft = await LoadDraftAsync(draftPublicId);
    return await InFreshScopeAsync<IReadOnlyList<Invitation>>(async sp =>
      await sp.GetRequiredService<GuestDraftsDbContext>()
        .Invitations.AsNoTracking()
        .Where(i => i.DraftId == draft.Id)
        .ToListAsync(Ct)
    );
  }

  protected Task<HttpResponseMessage> PostInviteAsync(string draftPublicId, string email, string name) =>
    HttpClient.PostAsJsonAsync(
      new Uri($"/guest-drafts/{draftPublicId}/invitations", UriKind.Relative),
      new { email, name },
      Ct
    );
}
