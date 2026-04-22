namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class AssignTriviaResultsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignTriviaResults_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();

    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 3
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AssignTriviaResults_ShouldPersistResultsInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();

    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 5
        },
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter2PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 2,
          QuestionsWon = 2
        }
      ]
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var triviaResults = await DbContext.TriviaResults
      .Where(tr => tr.DraftPart.PublicId == draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    triviaResults.Should().HaveCount(2);
    triviaResults.Should().Contain(tr => tr.Position == 1 && tr.QuestionsWon == 5);
    triviaResults.Should().Contain(tr => tr.Position == 2 && tr.QuestionsWon == 2);
  }

  [Fact]
  public async Task AssignTriviaResults_ShouldReplaceExistingResultsAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();

    // Assign results once
    await Sender.Send(new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 5
        }
      ]
    }, TestContext.Current.CancellationToken);

    // Assign again with different results
    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter2PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 7
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var triviaResults = await DbContext.TriviaResults
      .Where(tr => tr.DraftPart.PublicId == draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    triviaResults.Should().HaveCount(1);
    triviaResults[0].Position.Should().Be(1);
    triviaResults[0].QuestionsWon.Should().Be(7);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignTriviaResults_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = Faker.Random.AlphaNumeric(10),
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafterPublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 3
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
  public async Task AssignTriviaResults_WhenDraftPartNotStarted_ShouldFailAsync()
  {
    // Arrange — draft part exists but has NOT been started
    var (draftPartPublicId, drafterPublicId) = await SetupDraftPartWithoutStartingAsync();

    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafterPublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 3
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — duplicate participant in results
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignTriviaResults_WithDuplicateParticipant_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();

    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 5
        },
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 2,
          QuestionsWon = 2
        }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — participant not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignTriviaResults_WithNonExistentParticipant_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _, _) = await SetupStartedDraftPartAsync();

    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = Faker.Random.AlphaNumeric(10),
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 3
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

  private async Task<(string DraftPartPublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupStartedDraftPartAsync()
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

  private async Task<(string DraftPartPublicId, string DrafterPublicId)>
    SetupDraftPartWithoutStartingAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);
    var draftPartPublicId = draftPart.PublicId;

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personId), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, drafterPublicId);
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
