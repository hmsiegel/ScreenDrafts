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
      .Include("_vetoes.VetoOverride")
      .FirstAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

    pick.CurrentVeto.Should().NotBeNull();
    pick.CurrentVeto.IsOverridden.Should().BeTrue();
    pick.CurrentVeto.VetoOverride.Should().NotBeNull();
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

    return (draftPartPublicId, drafter1PublicId, drafter2PublicId);
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
    // Veto overrides are not allowed in Standard (or SpeedDraft) drafts, so use MiniMega here.
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.MiniMega.Value,
      SeriesId = seriesId,
    }, TestContext.Current.CancellationToken);

    // CreateDraft auto-creates part index 1 (min=1, max=7) when no Parts are supplied.
    return draftResult.Value;
  }
}
