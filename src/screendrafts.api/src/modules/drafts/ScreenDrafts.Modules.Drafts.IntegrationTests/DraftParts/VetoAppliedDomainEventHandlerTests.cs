using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

/// <summary>
/// Tests for VetoAppliedDomainEventHandler:
/// when a veto is applied to a pick, the movie should be restored to the DraftPool
/// (or re-added to the DraftBoard when no pool exists).
/// </summary>
public sealed class VetoAppliedDomainEventHandlerTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Pool path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task ApplyVeto_WhenPoolExists_ShouldRestoreMovieToPoolAsync()
  {
    // Arrange
    var tmdbId = Faker.Random.Int(1, 500_000);
    var (_, draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartWithPoolAsync(tmdbId);

    var movie = await DbContext.Movies.FirstAsync(m => m.TmdbId == tmdbId);

    // Play the pick — this removes movie from pool via PickCreatedDomainEventHandler
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

    // Act — apply veto to restore the movie to pool
    var result = await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    });
    await ProcessOutboxAsync();

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pool = await DbContext.DraftPools
      .AsNoTracking()
      .Include(p => p.TmdbIds)
      .FirstAsync();
    pool.TmdbIds.Should().Contain(i => i.TmdbId == tmdbId);
  }

  [Fact]
  public async Task ApplyVeto_ShouldPersistVetoInDatabaseAsync()
  {
    // Arrange
    var tmdbId = Faker.Random.Int(1, 500_000);
    var (_, draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartWithPoolAsync(tmdbId);

    var movie = await DbContext.Movies.FirstAsync(m => m.TmdbId == tmdbId);
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    // Act
    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    });

    // Assert — veto should be persisted on the pick
    var pick = await DbContext.Picks
      .Include(p => p.Veto)
      .FirstAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId);
    pick.Veto.Should().NotBeNull();
    pick.Veto!.IsOverridden.Should().BeFalse();
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private async Task<(string draftPublicId, string draftPartPublicId, string drafter1PublicId, string drafter2PublicId)>
    SetupStartedDraftPartWithPoolAsync(int tmdbId)
  {
    var draftPublicId = await CreateDraftWithPoolAsync();
    await CreateMovieInDbAsync(tmdbId);
    await Sender.Send(new AddMovieToDraftPoolCommand { PublicId = draftPublicId, TmdbId = tmdbId });

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

  private async Task<string> CreateDraftWithPoolAsync()
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

    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });
    return draftPublicId;
  }
}
