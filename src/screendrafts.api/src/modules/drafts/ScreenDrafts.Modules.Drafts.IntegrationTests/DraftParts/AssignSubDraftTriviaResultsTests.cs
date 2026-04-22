namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class AssignSubDraftTriviaResultsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignSubDraftTriviaResults_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, _) = await SetupStartedSpeedDraftPartAsync();

    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AssignSubDraftTriviaResults_ShouldActivateSubDraftAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, _) = await SetupStartedSpeedDraftPartAsync();

    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 0
        }
      ]
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var draftPart = await DbContext.DraftParts
      .Include("_subDrafts")
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

    var subDraft = draftPart.SubDrafts.First(s => s.PublicId == subDraftPublicId);
    subDraft.Status.Should().Be(SubDraftStatus.Active);
  }

  [Fact]
  public async Task AssignSubDraftTriviaResults_ShouldPersistTriviaResultsInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedSpeedDraftPartAsync();

    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        },
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter2PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 2,
          QuestionsWon = 0
        }
      ]
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var triviaResults = await DbContext.TriviaResults
      .AsNoTracking()
      .Where(tr => tr.DraftPart.PublicId == draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    triviaResults.Should().HaveCount(2);
    triviaResults.Should().Contain(tr => tr.Position == 1 && tr.QuestionsWon == 1);
    triviaResults.Should().Contain(tr => tr.Position == 2 && tr.QuestionsWon == 0);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignSubDraftTriviaResults_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = Faker.Random.AlphaNumeric(10),
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}",
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafterPublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — sub-draft not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignSubDraftTriviaResults_WithNonExistentSubDraft_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _, drafter1PublicId, _) = await SetupStartedSpeedDraftPartAsync();

    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}",
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not in progress
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignSubDraftTriviaResults_WhenDraftPartNotStarted_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId) = await SetupSpeedDraftPartWithSubDraftNotStartedAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafterPublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — sub-draft not pending (already active)
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignSubDraftTriviaResults_WhenSubDraftAlreadyActive_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, _) = await SetupStartedSpeedDraftPartAsync();

    // Activate the sub-draft first
    await Sender.Send(new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        }
      ]
    }, TestContext.Current.CancellationToken);

    // Try assigning again
    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string SubDraftPublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupStartedSpeedDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

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

    var subDraftPublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    }, TestContext.Current.CancellationToken)).Value;

    return (draftPartPublicId, subDraftPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<(string DraftPartPublicId, string SubDraftPublicId)>
    SetupSpeedDraftPartWithSubDraftNotStartedAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

    var subDraftPublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    }, TestContext.Current.CancellationToken)).Value;

    return (draftPartPublicId, subDraftPublicId);
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
      DefaultDraftType = DraftType.SpeedDraft.Value
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<string> CreateSpeedDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.SpeedDraft.Value,
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
