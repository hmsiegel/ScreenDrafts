using Npgsql;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations;

public sealed class InvitationRepositoryTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : InvitationIntegrationTest(factory)
{
  private async Task<Draft> CreateDraftAsync()
  {
    var owner = await CreateUserAsync();
    return await LoadDraftAsync(await CreateGuestDraftAsync(owner.UserPublicId));
  }

  private Task<T> Repo<T>(Func<IInvitationRepository, Task<T>> action) =>
    InFreshScopeAsync(sp => action(sp.GetRequiredService<IInvitationRepository>()));

  // ── Schema ────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Migration_ShouldBeAppliedAndLeaveNoPendingModelChangesAsync()
  {
    var applied = await DbContext.Database.GetAppliedMigrationsAsync(Ct);
    var pending = await DbContext.Database.GetPendingMigrationsAsync(Ct);

    applied.Should().Contain(m => m.EndsWith("Add_Draft_Invitations", StringComparison.Ordinal));
    pending.Should().BeEmpty();
    DbContext
      .Database.HasPendingModelChanges()
      .Should()
      .BeFalse("the Add_Draft_Invitations snapshot must match the current model");
  }

  [Fact]
  public async Task Table_ShouldExistInTheGuestDraftsSchemaWithTheExpectedColumnsAsync()
  {
    var columns = await DbContext
      .Database.SqlQuery<string>(
        $"""
        SELECT column_name AS "Value"
        FROM information_schema.columns
        WHERE table_schema = 'guest_drafts' AND table_name = 'draft_invitations'
        """
      )
      .ToListAsync(Ct);

    columns
      .Should()
      .BeEquivalentTo(
        "id",
        "public_id",
        "draft_id",
        "email",
        "name",
        "invited_by_user_id",
        "created_on_utc",
        "last_sent_on_utc",
        "accepted_on_utc",
        "accepted_by_user_id"
      );
  }

  [Fact]
  public async Task Indexes_ShouldIncludeAUniqueDraftEmailIndexAndAnEmailIndexAsync()
  {
    var indexes = await DbContext
      .Database.SqlQuery<string>(
        $"""
        SELECT indexdef AS "Value"
        FROM pg_indexes
        WHERE schemaname = 'guest_drafts' AND tablename = 'draft_invitations'
        """
      )
      .ToListAsync(Ct);

    indexes
      .Should()
      .Contain(i => i.Contains("UNIQUE", StringComparison.Ordinal) && i.Contains("(draft_id, email)", StringComparison.Ordinal));
    indexes
      .Should()
      .Contain(i => !i.Contains("UNIQUE", StringComparison.Ordinal) && i.Contains("(email)", StringComparison.Ordinal));
    indexes
      .Should()
      .Contain(i => i.Contains("UNIQUE", StringComparison.Ordinal) && i.Contains("(public_id)", StringComparison.Ordinal));
  }

  // ── Repository ────────────────────────────────────────────────────────────

  [Fact]
  public async Task AddThenGetById_ShouldRoundTripEveryFieldAsync()
  {
    var draft = await CreateDraftAsync();
    var added = await AddInvitationAsync(draft, "Round.Trip@Example.com", " Round Trip ");

    var loaded = await Repo(r => r.GetByIdAsync(added.Id, Ct));

    loaded.Should().NotBeNull();
    loaded.PublicId.Should().Be(added.PublicId);
    loaded.DraftId.Should().Be(draft.Id);
    loaded.Email.Should().Be("round.trip@example.com");
    loaded.Name.Should().Be("Round Trip");
    loaded.InvitedByUserId.Should().Be(draft.OwnerUserId);
    loaded.CreatedOnUtc.Should().Be(FixedNow);
    loaded.LastSentOnUtc.Should().Be(FixedNow);
    loaded.AcceptedOnUtc.Should().BeNull();
    loaded.AcceptedByUserId.Should().BeNull();
  }

  [Fact]
  public async Task GetByPublicId_ShouldReturnTheInvitation_AndNullForAnUnknownIdAsync()
  {
    var draft = await CreateDraftAsync();
    var added = await AddInvitationAsync(draft);

    var found = await Repo(r => r.GetByPublicIdAsync(added.PublicId, Ct));
    var missing = await Repo(r => r.GetByPublicIdAsync("gdi_doesnotexist000", Ct));

    found!.Id.Should().Be(added.Id);
    missing.Should().BeNull();
  }

  [Fact]
  public async Task GetByDraftAndEmail_ShouldMatchOnlyTheSameDraftAndEmailAsync()
  {
    var draft = await CreateDraftAsync();
    var otherDraft = await CreateDraftAsync();
    var added = await AddInvitationAsync(draft, "match@example.com");
    await AddInvitationAsync(otherDraft, "match@example.com");

    var hit = await Repo(r => r.GetByDraftAndEmailAsync(draft.Id.Value, "match@example.com", Ct));
    var wrongEmail = await Repo(r => r.GetByDraftAndEmailAsync(draft.Id.Value, "nope@example.com", Ct));

    hit!.Id.Should().Be(added.Id);
    wrongEmail.Should().BeNull();
  }

  [Fact]
  public async Task CountForDraft_ShouldCountEveryStatusButOnlyForThatDraftAsync()
  {
    var draft = await CreateDraftAsync();
    var otherDraft = await CreateDraftAsync();
    await AddInvitationAsync(draft, "a@example.com");
    var accepted = await AddInvitationAsync(draft, "b@example.com");
    await AddInvitationAsync(otherDraft, "c@example.com");
    accepted.Accept(Guid.NewGuid(), FixedNow.AddHours(1));
    await DbContext.SaveChangesAsync(Ct);

    var count = await Repo(r => r.CountForDraftAsync(draft.Id.Value, Ct));
    var otherCount = await Repo(r => r.CountForDraftAsync(otherDraft.Id.Value, Ct));
    var none = await Repo(r => r.CountForDraftAsync(Guid.NewGuid(), Ct));

    count.Should().Be(2);
    otherCount.Should().Be(1);
    none.Should().Be(0);
  }

  [Fact]
  public async Task GetPendingByEmail_ShouldExcludeAcceptedAndOtherEmailsAsync()
  {
    var first = await CreateDraftAsync();
    var second = await CreateDraftAsync();
    var pendingOne = await AddInvitationAsync(first, "target@example.com");
    var pendingTwo = await AddInvitationAsync(second, "target@example.com");
    var accepted = await AddInvitationAsync(await CreateDraftAsync(), "target@example.com");
    await AddInvitationAsync(first, "other@example.com");
    accepted.Accept(Guid.NewGuid(), FixedNow.AddHours(1));
    await DbContext.SaveChangesAsync(Ct);

    var pending = await Repo(r => r.GetPendingByEmailAsync("target@example.com", Ct));

    pending.Select(i => i.Id).Should().BeEquivalentTo([pendingOne.Id, pendingTwo.Id]);
  }

  [Fact]
  public async Task Update_ShouldPersistResendAndAcceptChangesAsync()
  {
    var draft = await CreateDraftAsync();
    var added = await AddInvitationAsync(draft);
    var user = Guid.NewGuid();

    await InFreshScopeAsync(async sp =>
    {
      var repo = sp.GetRequiredService<IInvitationRepository>();
      var invitation = (await repo.GetByIdAsync(added.Id, Ct))!;
      invitation.Resend(draft.PublicId, FixedNow.AddDays(2)).IsSuccess.Should().BeTrue();
      invitation.Accept(user, FixedNow.AddDays(3));
      repo.Update(invitation);
      await sp.GetRequiredService<GuestDraftsDbContext>().SaveChangesAsync(Ct);
      return 0;
    });

    var reloaded = await Repo(r => r.GetByIdAsync(added.Id, Ct));
    reloaded!.LastSentOnUtc.Should().Be(FixedNow.AddDays(2));
    reloaded.AcceptedOnUtc.Should().Be(FixedNow.AddDays(3));
    reloaded.AcceptedByUserId.Should().Be(user);
  }

  [Fact]
  public async Task Insert_ShouldViolateTheUniqueIndex_WhenTheSameDraftAndEmailAreInsertedTwiceAsync()
  {
    var draft = await CreateDraftAsync();
    await AddInvitationAsync(draft, "dup@example.com");

    var act = () => AddInvitationAsync(draft, "dup@example.com");

    var failure = await act.Should().ThrowAsync<DbUpdateException>();
    failure
      .Which.InnerException.Should()
      .BeOfType<PostgresException>()
      .Which.SqlState.Should()
      .Be(PostgresErrorCodes.UniqueViolation);
  }

  [Fact]
  public async Task Insert_ShouldAllowTheSameEmailOnDifferentDraftsAsync()
  {
    var first = await CreateDraftAsync();
    var second = await CreateDraftAsync();
    await AddInvitationAsync(first, "same@example.com");

    var act = () => AddInvitationAsync(second, "same@example.com");

    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task DeletingADraft_ShouldCascadeToItsInvitationsAsync()
  {
    var draft = await CreateDraftAsync();
    var survivorDraft = await CreateDraftAsync();
    await AddInvitationAsync(draft, "gone@example.com");
    var survivor = await AddInvitationAsync(survivorDraft, "stays@example.com");

    await DbContext.Database.ExecuteSqlAsync(
      $"DELETE FROM guest_drafts.drafts WHERE public_id = {draft.PublicId}",
      Ct
    );

    (await Repo(r => r.CountForDraftAsync(draft.Id.Value, Ct))).Should().Be(0);
    (await Repo(r => r.GetByIdAsync(survivor.Id, Ct))).Should().NotBeNull();
  }
}
