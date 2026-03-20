namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftPools;

/// <summary>
/// Tests for DraftPartStartedDomainEventHandler:
/// when a draft part starts, the draft pool should be locked.
/// </summary>
public sealed class DraftPartStartedDomainEventHandlerTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Pool locking
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task StartDraftPart_WhenPoolExists_ShouldLockPoolAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPoolAndSetupAsync();

    // Act
    await Sender.Send(new SetDraftPartStatusCommand
    {      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    });
    await ProcessOutboxAsync();

    // Assert — pool should now be locked
    var pool = await DbContext.DraftPools
      .AsNoTracking()
      .FirstAsync();
    pool.IsLocked.Should().BeTrue();
  }

  [Fact]
  public async Task StartDraftPart_WhenNoPoolExists_ShouldNotFailAsync()
  {
    // Arrange — draft WITHOUT pool
    var draftPublicId = await CreateDraftWithoutPoolAsync();
    var draftPartId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartId));

    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1PublicId = (await Sender.Send(new CreateDrafterCommand(person1Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPart.PublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPart.PublicId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId })).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPart.PublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    // Act — starting draft part when no pool exists should still succeed
    var result = await Sender.Send(new SetDraftPartStatusCommand
    {      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    });

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private async Task<(string draftPublicId, string draftPartPublicId)> CreateDraftWithPoolAndSetupAsync()
  {
    var draftPublicId = await CreateDraftAsync();
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    });

    var draftPartId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartId));
    var draftPartPublicId = draftPart.PublicId;

    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1PublicId = (await Sender.Send(new CreateDrafterCommand(person1Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId })).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    return (draftPublicId, draftPartPublicId);
  }

  private async Task<string> CreateDraftAsync()
  {
    var seriesResult = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });

    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesResult.Value,
    });

    return draftResult.Value;
  }

  private async Task<string> CreateDraftWithoutPoolAsync()
  {
    var draftPublicId = await CreateDraftAsync();
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
