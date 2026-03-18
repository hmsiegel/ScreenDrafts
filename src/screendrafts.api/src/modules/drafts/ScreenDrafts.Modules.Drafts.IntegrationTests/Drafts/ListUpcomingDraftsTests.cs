namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ListUpcomingDraftsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path — empty
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListUpcomingDrafts_WhenNoDraftsExist_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var query = new ListUpcomingDraftsQuery { UserId = Guid.NewGuid(), IsAdmin = false, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Happy path — returns drafts
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListUpcomingDrafts_WhenCreatedDraftExists_ShouldReturnItAsync()
  {
    // Arrange
    var (_, _) = await CreateDraftPartAsync();

    var query = new ListUpcomingDraftsQuery { UserId = Guid.NewGuid(), IsAdmin = false, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().HaveCount(1);
  }

  [Fact]
  public async Task ListUpcomingDrafts_WhenCompletedDraftExists_ShouldNotReturnItAsync()
  {
    // Arrange
    var (_, internalId) = await CreateDraftPartAsync();
    await DbContext.Database.ExecuteSqlRawAsync(
      "UPDATE drafts.draft_parts SET status = 3 WHERE id = {0}", internalId);

    var query = new ListUpcomingDraftsQuery { UserId = Guid.NewGuid(), IsAdmin = false, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Capabilities — admin
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListUpcomingDrafts_WithAdminFlag_ShouldReturnAdminCapabilitiesAsync()
  {
    // Arrange
    await CreateDraftPartAsync();

    var query = new ListUpcomingDraftsQuery { UserId = Guid.NewGuid(), IsAdmin = true, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var draft = result.Value.Drafts.Should().ContainSingle().Subject;
    draft.Capabilities.Role.Should().Be("Admin");
    draft.Capabilities.CanEdit.Should().BeTrue();
    draft.Capabilities.CanDelete.Should().BeTrue();
    draft.Capabilities.CanStart.Should().BeTrue();
    draft.Capabilities.CanUpdateBoard.Should().BeTrue();
    draft.Capabilities.CanJoin.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Capabilities — host (commissioner)
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListUpcomingDrafts_WhenUserIsHost_ShouldReturnCommissionerCapabilitiesAsync()
  {
    // Arrange
    var (draftPartPublicId, _) = await CreateDraftPartAsync();
    var userId = await CreatePersonWithUserIdAndAddAsHostAsync(draftPartPublicId);

    var query = new ListUpcomingDraftsQuery { UserId = userId, IsAdmin = false, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var draft = result.Value.Drafts.Should().ContainSingle().Subject;
    draft.Capabilities.Role.Should().Be("Commissioner");
    draft.Capabilities.CanEdit.Should().BeTrue();
    draft.Capabilities.CanStart.Should().BeTrue();
    draft.Capabilities.CanJoin.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Capabilities — participant (drafter)
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListUpcomingDrafts_WhenUserIsParticipant_ShouldReturnDrafterCapabilitiesAsync()
  {
    // Arrange
    var (draftPartPublicId, _) = await CreateDraftPartAsync();
    var userId = await CreatePersonWithUserIdAndAddAsParticipantAsync(draftPartPublicId);

    var query = new ListUpcomingDraftsQuery { UserId = userId, IsAdmin = false, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var draft = result.Value.Drafts.Should().ContainSingle().Subject;
    draft.Capabilities.Role.Should().Be("Drafter");
    draft.Capabilities.CanEdit.Should().BeFalse();
    draft.Capabilities.CanDelete.Should().BeFalse();
    draft.Capabilities.CanStart.Should().BeFalse();
    draft.Capabilities.CanUpdateBoard.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Capabilities — guest
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListUpcomingDrafts_WhenUserIsGuest_ShouldReturnGuestCapabilitiesAsync()
  {
    // Arrange
    await CreateDraftPartAsync();

    var query = new ListUpcomingDraftsQuery { UserId = Guid.NewGuid(), IsAdmin = false, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var draft = result.Value.Drafts.Should().ContainSingle().Subject;
    draft.Capabilities.Role.Should().BeNull();
    draft.Capabilities.CanEdit.Should().BeFalse();
    draft.Capabilities.CanDelete.Should().BeFalse();
    draft.Capabilities.CanStart.Should().BeFalse();
    draft.Capabilities.CanUpdateBoard.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string PublicId, Guid InternalId)> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftWithPartAsync(seriesId);
    var internalId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(internalId));
    return (draftPart.PublicId, internalId);
  }

  private async Task<Guid> CreatePersonWithUserIdAndAddAsHostAsync(string draftPartPublicId)
  {
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personPublicId = await peopleFactory.CreateAndSavePersonAsync();
    var userId = Guid.NewGuid();

    await DbContext.Database.ExecuteSqlRawAsync(
      "UPDATE drafts.people SET user_id = {0} WHERE public_id = {1}", userId, personPublicId);

    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personPublicId })).Value;

    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    return userId;
  }

  private async Task<Guid> CreatePersonWithUserIdAndAddAsParticipantAsync(string draftPartPublicId)
  {
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personPublicId = await peopleFactory.CreateAndSavePersonAsync();
    var userId = Guid.NewGuid();

    await DbContext.Database.ExecuteSqlRawAsync(
      "UPDATE drafts.people SET user_id = {0} WHERE public_id = {1}", userId, personPublicId);

    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personPublicId))).Value;

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    return userId;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });

    return result.Value;
  }

  private async Task<string> CreateDraftWithPartAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    });

    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    });

    return draftPublicId;
  }
}
