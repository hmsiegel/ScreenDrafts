using ScreenDrafts.Common.Application.Services;
using ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class RevealPickTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  private readonly IPublicIdGenerator _publicIdGenerator = factory.Services.GetRequiredService<IPublicIdGenerator>();

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _, hostUserPublicId) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Act
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      UserPublicId = hostUserPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RevealPick_ShouldPersistRevealedAtAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _, hostUserPublicId) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Act
    await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      UserPublicId = hostUserPublicId
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
    // Act — neither the draft part nor the caller need to resolve to anything
    // real; the draft-part lookup fails before identity is ever touched.
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPart),
      PlayOrder = 1,
      UserPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.User)
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftPartErrors.NotFound(string.Empty).Code);
  }

  // -------------------------------------------------------------------------
  // Guard — pick not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenPickNotFoundAsync()
  {
    // Arrange
    var (draftPartPublicId, _, _, hostUserPublicId) = await SetupAsync();

    // Act — no picks played; play order 1 does not exist
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      UserPublicId = hostUserPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftPartErrors.PickNotFound(1).Code);
  }

  // -------------------------------------------------------------------------
  // Guard — caller's User has no linked Person
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenUserHasNoLinkedPersonAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _, _) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // A User exists in the fake Users module but was never linked to a Person.
    var orphanUserId = Guid.NewGuid();
    var orphanUserPublicId = FakeUsersApi.RegisterUser(orphanUserId, $"u_{Faker.Random.AlphaNumeric(16)}");

    // Act
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      UserPublicId = orphanUserPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == PersonErrors.NotFoundForUser(orphanUserPublicId).Code);
  }

  // -------------------------------------------------------------------------
  // Guard — caller is not primary host
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenCallerIsNotPrimaryHostAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _, _) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Drafter1 has a real User->Person link but is not a Host at all, let
    // alone the primary host — this exercises the authorization check itself
    // rather than failing earlier in identity resolution.
    var drafter1UserPublicId = await LinkPersonBehindParticipantToNewUserAsync(drafter1PublicId);

    // Act
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      UserPublicId = drafter1UserPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == HostErrors.NotFoundForPerson(string.Empty).Code
      || e.Code == DraftPartErrors.OnlyPrimaryHostCanRevealPicks.Code);
  }

  // -------------------------------------------------------------------------
  // Guard — pick already revealed
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RevealPick_ShouldFail_WhenPickAlreadyRevealedAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _, hostUserPublicId) = await SetupAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      UserPublicId = hostUserPublicId
    }, TestContext.Current.CancellationToken);

    // Act — reveal again
    var result = await Sender.Send(new RevealPickCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      UserPublicId = hostUserPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == PickErrors.PickAlreadyRevealed.Code);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  /// <summary>
  /// Sets up a draft part with two drafters and a primary host. Returns the
  /// host's UserPublicId (not the host's own domain public id) since that is
  /// what RevealPickCommand actually requires — the field the Endpoint
  /// populates from the JWT via User.GetUserPublicId().
  /// </summary>
  private async Task<(string DraftPartPublicId, string Drafter1PublicId, string Drafter2PublicId, string HostUserPublicId)> SetupAsync()
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

    // Link the host's Person to a fake User so the handler's
    // User -> Person -> Host chain has something real to resolve.
    var hostUserPublicId = await LinkPersonToNewUserAsync(hostPersonId);

    await Sender.Send(new SetDraftPartStatusCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, drafter1PublicId, drafter2PublicId, hostUserPublicId);
  }

  /// <summary>
  /// Registers a fake User and links it to the given Person via
  /// LinkUserPersonCommand. Returns the new User's public id.
  /// </summary>
  private async Task<string> LinkPersonToNewUserAsync(string personPublicId)
  {
    var userId = Guid.NewGuid();
    var userPublicId = FakeUsersApi.RegisterUser(userId, $"u_{Faker.Random.AlphaNumeric(16)}");

    var linkResult = await Sender.Send(new LinkUserPersonCommand
    {
      PublicId = personPublicId,
      UserId = userId
    }, TestContext.Current.CancellationToken);

    linkResult.IsSuccess.Should().BeTrue("test setup must be able to link a fresh Person to a fresh User");

    return userPublicId;
  }

  /// <summary>
  /// Looks up the Person backing a Drafter participant and links it to a new
  /// fake User. Used for guard tests where the caller has a valid identity
  /// chain but lacks the Host role entirely.
  /// </summary>
  private async Task<string> LinkPersonBehindParticipantToNewUserAsync(string drafterPublicId)
  {
    var personPublicId = await DbContext.Drafters
      .Where(d => d.PublicId == drafterPublicId)
      .Select(d => d.Person.PublicId)
      .FirstAsync(TestContext.Current.CancellationToken);

    return await LinkPersonToNewUserAsync(personPublicId);
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

  private async Task<string> CreateSeriesAsync()
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

  private async Task<string> CreateDraftAsync(string seriesId)
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
