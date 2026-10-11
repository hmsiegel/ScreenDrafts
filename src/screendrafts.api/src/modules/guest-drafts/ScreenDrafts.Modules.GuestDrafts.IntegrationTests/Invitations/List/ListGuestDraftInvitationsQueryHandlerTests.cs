namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations.List;

/// <summary>
/// Runs the List query's raw SQL against the real schema, so a snake_case column or table
/// mismatch fails here instead of in production.
/// </summary>
public sealed class ListGuestDraftInvitationsQueryHandlerTests(
  GuestDraftsIntegrationTestWebAppFactory factory
) : InvitationIntegrationTest(factory)
{
  private static readonly TimeSpan Cooldown = TimeSpan.FromHours(24);

  private Task<Result<ListGuestDraftInvitationsResponse>> ListAsync(
    string draftPublicId,
    string callerUserPublicId
  ) =>
    Sender.Send(
      new ListGuestDraftInvitationsQuery
      {
        GuestDraftPublicId = draftPublicId,
        CallerUserPublicId = callerUserPublicId,
      },
      Ct
    );

  private async Task<(TestUser Owner, string DraftPublicId)> CreateDraftWithOwnerAsync()
  {
    Clock.Pin(FixedNow);
    var owner = await CreateUserAsync();
    return (owner, await CreateGuestDraftAsync(owner.UserPublicId));
  }

  [Fact]
  public async Task Handle_ShouldReturnCallerNotFound_WhenCallerIsUnknownAsync()
  {
    var (_, draftPublicId) = await CreateDraftWithOwnerAsync();

    var result = await ListAsync(draftPublicId, "u_unknownunknown01");

    result.Error!.Code.Should().Be("UserErrors.PublicIdNotFound");
  }

  [Fact]
  public async Task Handle_ShouldReturnDraftNotFound_WhenDraftIsUnknownAsync()
  {
    var owner = await CreateUserAsync();

    var result = await ListAsync("gd_doesnotexist0000", owner.UserPublicId);

    result.Error!.Code.Should().Be("Drafts.NotFound");
  }

  [Fact]
  public async Task Handle_ShouldReturnOnlyOwnerCanPerformThisAction_WhenCallerIsNotTheOwnerAsync()
  {
    var (_, draftPublicId) = await CreateDraftWithOwnerAsync();
    var stranger = await CreateUserAsync();

    var result = await ListAsync(draftPublicId, stranger.UserPublicId);

    result.Error.Should().Be(DraftErrors.OnlyOwnerCanPerformThisAction);
  }

  [Fact]
  public async Task Handle_ShouldReturnAnEmptyList_WhenNobodyWasInvitedAsync()
  {
    var (owner, draftPublicId) = await CreateDraftWithOwnerAsync();

    var result = await ListAsync(draftPublicId, owner.UserPublicId);

    result.IsSuccess.Should().BeTrue();
    result.Value.Invitations.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReportPendingWithinCooldownAsNotResendableAsync()
  {
    var (owner, draftPublicId) = await CreateDraftWithOwnerAsync();
    var invited = await InviteAsync(draftPublicId, owner.UserPublicId, "Pat@Example.com", "Pat Pending");
    Clock.Advance(Cooldown - TimeSpan.FromSeconds(1));

    var result = await ListAsync(draftPublicId, owner.UserPublicId);

    var row = result.Value.Invitations.Should().ContainSingle().Subject;
    row.PublicId.Should().Be(invited.Value.PublicId);
    row.Email.Should().Be("pat@example.com");
    row.Name.Should().Be("Pat Pending");
    row.Status.Should().Be("Pending");
    row.SentOnUtc.Should().Be(FixedNow);
    row.CanResend.Should().BeFalse();
    row.ResendAvailableOnUtc.Should().Be(FixedNow + Cooldown);
  }

  [Fact]
  public async Task Handle_ShouldReportPendingAfterCooldownAsResendableAsync()
  {
    var (owner, draftPublicId) = await CreateDraftWithOwnerAsync();
    await InviteAsync(draftPublicId, owner.UserPublicId);
    Clock.Advance(Cooldown);

    var result = await ListAsync(draftPublicId, owner.UserPublicId);

    var row = result.Value.Invitations.Should().ContainSingle().Subject;
    row.Status.Should().Be("Pending");
    row.CanResend.Should().BeTrue();
    row.ResendAvailableOnUtc.Should().Be(FixedNow + Cooldown);
  }

  [Fact]
  public async Task Handle_ShouldReportAcceptedAsNotResendableWithoutAResendTimeAsync()
  {
    var (owner, draftPublicId) = await CreateDraftWithOwnerAsync();
    await InviteAsync(draftPublicId, owner.UserPublicId, "done@example.com", "Dana Done");
    var tracked = await DbContext.Invitations.SingleAsync(Ct);
    tracked.Accept(Guid.NewGuid(), FixedNow.AddHours(1));
    await DbContext.SaveChangesAsync(Ct);
    Clock.Advance(TimeSpan.FromDays(10));

    var result = await ListAsync(draftPublicId, owner.UserPublicId);

    var row = result.Value.Invitations.Should().ContainSingle().Subject;
    row.Status.Should().Be("Accepted");
    row.CanResend.Should().BeFalse();
    row.ResendAvailableOnUtc.Should().BeNull();
  }

  [Fact]
  public async Task Handle_ShouldListOldestFirstAndOnlyForThisDraftAsync()
  {
    var (owner, draftPublicId) = await CreateDraftWithOwnerAsync();
    var otherDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    await InviteAsync(draftPublicId, owner.UserPublicId, "first@example.com", "First");
    Clock.Advance(TimeSpan.FromMinutes(1));
    await InviteAsync(draftPublicId, owner.UserPublicId, "second@example.com", "Second");
    await InviteAsync(otherDraftPublicId, owner.UserPublicId, "elsewhere@example.com", "Elsewhere");

    var result = await ListAsync(draftPublicId, owner.UserPublicId);

    result.Value.Invitations.Select(i => i.Email).Should().Equal("first@example.com", "second@example.com");
  }
}
