using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

/// <summary>
/// Tests for VetoOverrideAppliedDomainEventHandler:
/// when a veto override is applied, the movie should be removed from the DraftPool
/// (same behavior as when the pick was first played).
/// </summary>
public sealed class VetoOverrideAppliedDomainEventHandlerTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Pool path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task ApplyVetoOverride_WhenPoolExists_ShouldRemoveMovieFromPoolAsync()
  {
    // Arrange
    var tmdbId = Faker.Random.Int(1, 500_000);
    var (_, draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartWithPoolAsync(tmdbId);

    var movie = await DbContext.Movies.FirstAsync(m => m.TmdbId == tmdbId, TestContext.Current.CancellationToken);

    // Play pick → movie removed from pool
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    }, TestContext.Current.CancellationToken);
    await ProcessOutboxAsync();

    // Apply veto → movie restored to pool
    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    }, TestContext.Current.CancellationToken);
    await ProcessOutboxAsync();

    // Act — apply veto override → movie should be removed from pool again
    // (the overriding participant must differ from the original picker, drafter1)
    var result = await Sender.Send(new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    }, TestContext.Current.CancellationToken);
    await ProcessOutboxAsync();

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pool = await DbContext.DraftPools
      .AsNoTracking()
      .Include(p => p.TmdbIds)
      .FirstAsync(TestContext.Current.CancellationToken);
    pool.TmdbIds.Should().NotContain(i => i.TmdbId == tmdbId);
  }

  [Fact]
  public async Task ApplyVetoOverride_ShouldPersistOverrideInDatabaseAsync()
  {
    // Arrange
    var tmdbId = Faker.Random.Int(1, 500_000);
    var (_, draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartWithPoolAsync(tmdbId);

    var movie = await DbContext.Movies.FirstAsync(m => m.TmdbId == tmdbId, TestContext.Current.CancellationToken);
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    }, TestContext.Current.CancellationToken);
    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    }, TestContext.Current.CancellationToken);

    // Act — the overriding participant must differ from the original picker, drafter1
    await Sender.Send(new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    }, TestContext.Current.CancellationToken);

    // Assert — veto override should mark veto as overridden
    var pick = await DbContext.Picks
      .Include("_vetoes")
      .FirstAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);
    pick.CurrentVeto.Should().NotBeNull();
    pick.CurrentVeto.IsOverridden.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private async Task<(string draftPublicId, string draftPartPublicId, string drafter1PublicId, string drafter2PublicId)>
    SetupStartedDraftPartWithPoolAsync(int tmdbId)
  {
    var draftPublicId = await CreateDraftWithPoolAsync();
    await CreateMovieInDbAsync(tmdbId);
    await Sender.Send(new AddMovieToDraftPoolCommand { PublicId = draftPublicId, TmdbId = tmdbId, MediaType = MediaType.Movie }, TestContext.Current.CancellationToken);

    // CreateDraft auto-creates part index 1 (min=1, max=7) when no Parts are supplied.
    var draftPartId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartId), TestContext.Current.CancellationToken);
    var draftPartPublicId = draftPart.PublicId;

    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1PublicId = (await Sender.Send(new CreateDrafterCommand(person1Id), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    // A third drafter is required — SeriesPolicyRules.ComputeMaxVetoOverrides grants zero
    // veto overrides to exactly-2-participant Mega/Super/mini-Mega drafts.
    var person3Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter3PublicId = (await Sender.Send(new CreateDrafterCommand(person3Id), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter3PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId }, TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new SetDraftPartStatusCommand
    {      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    }, TestContext.Current.CancellationToken);

    // Veto overrides are only ever usable if explicitly granted — there is no baseline
    // "starting" override the way there is for vetoes. Grant drafter2 a bonus override
    // via a draft position, mirroring how HasBonusVeto works in the Sorkin scenario.
    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "P1", Picks = [1] },
        new DraftPositionRequest { Name = "P2", Picks = [2], HasBonusVetoOverride = true },
        new DraftPositionRequest { Name = "P3", Picks = [3] },
      ]
    }, TestContext.Current.CancellationToken);

    var p1PositionId = await GetPositionPublicIdByNameAsync(draftPartPublicId, "P1");
    var p2PositionId = await GetPositionPublicIdByNameAsync(draftPartPublicId, "P2");
    var p3PositionId = await GetPositionPublicIdByNameAsync(draftPartPublicId, "P3");

    await Sender.Send(new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = p1PositionId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);
    await Sender.Send(new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = p2PositionId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);
    await Sender.Send(new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = p3PositionId,
      ParticipantPublicId = drafter3PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    return (draftPublicId, draftPartPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<string> GetPositionPublicIdByNameAsync(string draftPartPublicId, string positionName)
  {
    var partId = await DbContext.DraftParts
      .Where(dp => dp.PublicId == draftPartPublicId)
      .Select(dp => dp.Id)
      .FirstAsync(TestContext.Current.CancellationToken);

    return await DbContext.DraftPositions
      .Where(pos => pos.GameBoard.DraftPartId == partId && pos.Name == positionName)
      .Select(pos => pos.PublicId)
      .FirstAsync(TestContext.Current.CancellationToken);
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
    }, TestContext.Current.CancellationToken);

    // Veto overrides are not allowed in Standard (or SpeedDraft) drafts, so use MiniMega here.
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.MiniMega.Value,
      SeriesId = seriesResult.Value,
    }, TestContext.Current.CancellationToken);

    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId }, TestContext.Current.CancellationToken);
    return draftPublicId;
  }
}
