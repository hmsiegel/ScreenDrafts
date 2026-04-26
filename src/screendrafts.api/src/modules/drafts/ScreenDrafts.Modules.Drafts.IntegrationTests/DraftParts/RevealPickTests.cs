namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class RevealPickTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, hostPublicId) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Act
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ActorPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RevealPick_ShouldPersistRevealedAtAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, hostPublicId) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Act
    await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ActorPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    var pick = await DbContext.Picks
      .FirstAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

    pick.RevealedAt.Should().NotBeNull();
    pick.IsRevealed.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Act
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = Faker.Random.AlphaNumeric(12),
      PlayOrder = 1,
      ActorPublicId = Faker.Random.AlphaNumeric(12)
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — pick not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenPickNotFoundAsync()
  {
    // Arrange
    var (draftPartPublicId, _, hostPublicId) = await SetupAsync();

    // Act — no picks played; play order 1 does not exist
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ActorPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftPartErrors.PickNotFound(1).Code);
  }

  // -------------------------------------------------------------------------
  // Guard — caller is not primary host
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenCallerIsNotPrimaryHostAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Act — use the drafter's public ID instead of the host's
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ActorPublicId = drafter1PublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftPartErrors.OnlyPrimaryHostCanRevealPicks.Code);
  }

  // -------------------------------------------------------------------------
  // Guard — pick already revealed
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenPickAlreadyRevealedAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, hostPublicId) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ActorPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    // Act — reveal again
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ActorPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == PickErrors.PickAlreadyRevealed.Code);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string Drafter1PublicId, string HostPublicId)> SetupAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);
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

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId }, TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new SetDraftPartStatusCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, drafter1PublicId, hostPublicId);
  }

  private async Task PlayPickAsync(string draftPartPublicId, string drafterPublicId, int position, int playOrder, Movie movie)
  {
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = position,
      PlayOrder = playOrder,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    }, TestContext.Current.CancellationToken);
  }

  private async Task<Movie> CreateMovieAsync()
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), $"m_{Faker.Random.AlphaNumeric(21)}", MediaType.Movie, Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    return movie;
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
    }, TestContext.Current.CancellationToken);

    return result.Value;
  }

  private async Task<string> CreateDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    }, TestContext.Current.CancellationToken);

    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    }, TestContext.Current.CancellationToken);

    return draftPublicId;
  }
}
