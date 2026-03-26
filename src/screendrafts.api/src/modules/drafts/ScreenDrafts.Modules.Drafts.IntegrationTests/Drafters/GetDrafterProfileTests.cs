using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;
using ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;
using ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public sealed class GetDrafterProfileTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetDrafterProfile_WithInvalidId_ShouldReturnNotFoundAsync()
  {
    // Arrange
    var invalidId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = await Sender.Send(new GetDrafterProfileQuery { PublicId = invalidId });

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // New drafter with no draft participation
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetDrafterProfile_WithNewDrafter_ShouldReturnEmptyProfileAsync()
  {
    // Arrange
    var drafterPublicId = await CreateDrafterAsync();

    // Act
    var result = await Sender.Send(new GetDrafterProfileQuery { PublicId = drafterPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DrafterPublicId.Should().Be(drafterPublicId);
    result.Value.DisplayName.Should().NotBeNullOrEmpty();
    result.Value.PersonPublicId.Should().NotBeNullOrEmpty();
    result.Value.TotalDrafts.Should().Be(0);
    result.Value.FilmsDrafted.Should().Be(0);
    result.Value.VetoesUsed.Should().Be(0);
    result.Value.VetoOverridesUsed.Should().Be(0);
    result.Value.CommissionerOverrides.Should().Be(0);
    result.Value.TimesVetoed.Should().Be(0);
    result.Value.TimeesVetoOverridden.Should().Be(0);
    result.Value.HasRolloverVeto.Should().BeFalse();
    result.Value.HasRolloverVetoOverride.Should().BeFalse();
    result.Value.FirstDraft.Should().BeNull();
    result.Value.MostRecentDraft.Should().BeNull();
    result.Value.SocialHandles.Should().BeNull();
    result.Value.DraftHistory.Should().BeEmpty();
    result.Value.VetoHistory.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Drafter with a pick
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetDrafterProfile_WithPickHistory_ShouldReturnPicksAsync()
  {
    // Arrange
    var (draftPartPublicId, draftPublicId, drafterPublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    // Act
    var result = await Sender.Send(new GetDrafterProfileQuery { PublicId = drafterPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DrafterPublicId.Should().Be(drafterPublicId);
    result.Value.FilmsDrafted.Should().Be(1);
    result.Value.FirstDraft.Should().NotBeNull();
    result.Value.FirstDraft!.DraftPublicId.Should().Be(draftPublicId);
    result.Value.MostRecentDraft.Should().NotBeNull();
    result.Value.DraftHistory.Should().HaveCount(1);
    result.Value.DraftHistory[0].Picks.Should().HaveCount(1);
    result.Value.DraftHistory[0].Picks[0].Position.Should().Be(1);
    result.Value.DraftHistory[0].Picks[0].PlayOrder.Should().Be(1);
    result.Value.DraftHistory[0].Picks[0].MovieTitle.Should().Be(movie.MovieTitle);
    result.Value.DraftHistory[0].Picks[0].WasVetoed.Should().BeFalse();
    result.Value.VetoHistory.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Drafter with a vetoed pick
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetDrafterProfile_WithVetoedPick_ShouldReflectInStatsAsync()
  {
    // Arrange — drafter1 plays a pick, drafter1 vetoes it (self-veto is allowed by tests)
    var (draftPartPublicId, _, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    // drafter2 vetoes drafter1's pick
    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    });

    // Act — check drafter1's profile (the one whose pick was vetoed)
    var result = await Sender.Send(new GetDrafterProfileQuery { PublicId = drafter1PublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TimesVetoed.Should().Be(1);
    result.Value.DraftHistory.Should().HaveCount(1);
    result.Value.DraftHistory[0].Picks[0].WasVetoed.Should().BeTrue();
    result.Value.DraftHistory[0].Picks[0].WasVetoOverridden.Should().BeFalse();

    // Act — check drafter2's profile (the one who issued the veto)
    var vetoerResult = await Sender.Send(new GetDrafterProfileQuery { PublicId = drafter2PublicId });

    // Assert
    vetoerResult.IsSuccess.Should().BeTrue();
    vetoerResult.Value.VetoHistory.Should().HaveCount(1);
    vetoerResult.Value.VetoHistory[0].TargetDrafterPublicId.Should().Be(drafter1PublicId);
    vetoerResult.Value.VetoHistory[0].WasVetoOverridden.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<string> CreateDrafterAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var result = await Sender.Send(new CreateDrafterCommand(personId));
    return result.Value;
  }

  private async Task<(string DraftPartPublicId, string DraftPublicId, string Drafter1PublicId, string Drafter2PublicId)> SetupStartedDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));
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

    return (draftPartPublicId, draftPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<Movie> CreateMovieAsync()
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), Faker.Random.AlphaNumeric(10), MediaType.Movie, Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync();
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
    });

    return result.Value;
  }

  private async Task<string> CreateDraftAsync(Guid seriesId)
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
