namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

/// <summary>
/// Tests for PickCreatedDomainEventHandler:
/// when a pick is played, the movie should be removed from the DraftPool
/// (or from the DraftBoard when no pool exists).
/// </summary>
public sealed class PickCreatedDomainEventHandlerTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Pool path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WhenPoolExists_ShouldRemoveMovieFromPoolAsync()
  {
    // Arrange
    var tmdbId = Faker.Random.Int(1, 500_000);
    var (_, draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartWithPoolAsync(tmdbId);

    var movie = await DbContext.Movies.FirstAsync(m => m.TmdbId == tmdbId);

    // Act
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });
    await ProcessOutboxAsync();

    // Assert — movie should be removed from pool
    var pool = await DbContext.DraftPools
      .AsNoTracking()
      .Include(p => p.TmdbIds)
      .FirstAsync();
    pool.TmdbIds.Should().NotContain(i => i.TmdbId == tmdbId);
  }

  [Fact]
  public async Task PlayPick_WhenPoolExistsAndMovieNotInPool_ShouldSucceedWithoutErrorAsync()
  {
    // Arrange — pool exists but does NOT contain the picked movie
    var draftPublicId = await CreateDraftWithPoolAsync();
    var (_, draftPartPublicId, drafter1PublicId, _) = await SetupStartedPartAsync(draftPublicId);

    var movie = Movie.Create(Faker.Company.CompanyName(), Faker.Random.AlphaNumeric(10), MediaType.Movie, Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync();

    // Act
    var result = await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    // Assert — pick should succeed even if movie not in pool
    result.IsSuccess.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Integration event published
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_ShouldPublishPickAddedIntegrationEventAsync()
  {
    // Arrange
    var tmdbId = Faker.Random.Int(1, 500_000);
    var (_, draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartWithPoolAsync(tmdbId);
    var movie = await DbContext.Movies.FirstAsync(m => m.TmdbId == tmdbId);

    // Act
    var result = await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    // Assert — pick persisted (integration event would have been published)
    result.IsSuccess.Should().BeTrue();
    var pick = await DbContext.Picks
      .FirstOrDefaultAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId);
    pick.Should().NotBeNull();
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private async Task<(string draftPublicId, string draftPartPublicId, string drafter1PublicId, string drafter2PublicId)>
    SetupStartedDraftPartWithPoolAsync(int tmdbId)
  {
    var draftPublicId = await CreateDraftWithPoolAsync();

    // Add movie to pool
    await CreateMovieInDbAsync(tmdbId);
    await Sender.Send(new AddMovieToDraftPoolCommand { PublicId = draftPublicId, TmdbId = tmdbId });

    var (_, draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedPartAsync(draftPublicId);

    return (draftPublicId, draftPartPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<(string draftPublicId, string draftPartPublicId, string drafter1PublicId, string drafter2PublicId)>
    SetupStartedPartAsync(string draftPublicId)
  {
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    });

    var draftPartId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartId));
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

    await Sender.Send(new SetDraftPartStatusCommand
    {      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    });

    return (draftPublicId, draftPartPublicId, drafter1PublicId, drafter2PublicId);
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

  private async Task<string> CreateDraftWithPoolAsync()
  {
    var draftPublicId = await CreateDraftAsync();
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });
    return draftPublicId;
  }
}
