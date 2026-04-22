using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class ApplyVetoOverrideTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ApplyVetoOverride_WithValidData_ShouldSucceedAsync()
  {
    // Arrange — start part, play pick, apply veto, then override the veto
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    }, TestContext.Current.CancellationToken);

    var command = new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ApplyVetoOverride_ShouldPersistOverrideAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    }, TestContext.Current.CancellationToken);

    var command = new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert — veto should be marked as overridden in the database
    var pick = await DbContext.Picks
      .Include(p => p.Veto)
        .ThenInclude(v => v!.VetoOverride)
      .FirstAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

    pick.Veto.Should().NotBeNull();
    pick.Veto!.IsOverridden.Should().BeTrue();
    pick.Veto.VetoOverride.Should().NotBeNull();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ApplyVetoOverride_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new ApplyVetoOverrideCommand
    {
      DraftPartId = Faker.Random.AlphaNumeric(10),
      PlayOrder = 1,
      ParticipantIdValue = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafterPublicId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — pick not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ApplyVetoOverride_WithNonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    }, TestContext.Current.CancellationToken);

    var command = new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 999,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — veto not found on pick
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ApplyVetoOverride_WhenPickHasNoVeto_ShouldFailAsync()
  {
    // Arrange — play a pick but do NOT apply a veto
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1);

    var command = new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — veto already overridden
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ApplyVetoOverride_WhenVetoAlreadyOverridden_ShouldFailAsync()
  {
    // Arrange — apply veto and then override it; try to override again
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    }, TestContext.Current.CancellationToken);

    // First override — should succeed
    await Sender.Send(new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    }, TestContext.Current.CancellationToken);

    // Second override on the same veto — should fail
    var command = new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string Drafter1PublicId, string Drafter2PublicId)> SetupStartedDraftPartAsync()
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
    {      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task PlayPickAsync(string draftPartPublicId, string drafterPublicId, int position, int playOrder)
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), $"m_{Faker.Random.AlphaNumeric(21)}", MediaType.Movie, Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = position,
      PlayOrder = playOrder,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    };

    await Sender.Send(command, TestContext.Current.CancellationToken);
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
