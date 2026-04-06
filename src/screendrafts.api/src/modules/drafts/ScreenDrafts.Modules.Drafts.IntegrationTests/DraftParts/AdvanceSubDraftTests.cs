namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class AdvanceSubDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AdvanceSubDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, _, _) = await SetupActiveSubDraftAsync();

    var command = new AdvanceSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AdvanceSubDraft_ShouldMarkSubDraftAsCompletedAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, _, _) = await SetupActiveSubDraftAsync();

    // Act
    await Sender.Send(new AdvanceSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId
    });

    // Assert
    var draftPart = await DbContext.DraftParts
      .Include("_subDrafts")
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == draftPartPublicId);

    var subDraft = draftPart.SubDrafts.First(s => s.PublicId == subDraftPublicId);
    subDraft.Status.Should().Be(SubDraftStatus.Completed);
  }

  [Fact]
  public async Task AdvanceSubDraft_WithTwoSubDrafts_ShouldSucceedAndCarryVetoesAsync()
  {
    // Arrange: set up two sub-drafts, activate the first, advance it to the second
    var (draftPartPublicId, subDraft1PublicId, _, _) =
      await SetupTwoSubDraftsWithFirstActiveAsync();

    // Act
    var result = await Sender.Send(new AdvanceSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraft1PublicId
    });

    // Assert
    result.IsSuccess.Should().BeTrue();

    var draftPart = await DbContext.DraftParts
      .Include("_subDrafts")
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == draftPartPublicId);

    var firstSubDraft = draftPart.SubDrafts.First(s => s.PublicId == subDraft1PublicId);
    firstSubDraft.Status.Should().Be(SubDraftStatus.Completed);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AdvanceSubDraft_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new AdvanceSubDraftCommand
    {
      DraftPartPublicId = Faker.Random.AlphaNumeric(10),
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — sub-draft not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AdvanceSubDraft_WithNonExistentSubDraft_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _, _, _) = await SetupActiveSubDraftAsync();

    var command = new AdvanceSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — sub-draft not active (pending)
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AdvanceSubDraft_WhenSubDraftIsPending_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, _, _) = await SetupPendingSubDraftAsync();

    var command = new AdvanceSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string SubDraftPublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupActiveSubDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

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
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    });

    var subDraftPublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    })).Value;

    // Activate the sub-draft via trivia assignment
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
    });

    return (draftPartPublicId, subDraftPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<(string DraftPartPublicId, string SubDraft1PublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupTwoSubDraftsWithFirstActiveAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

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
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    });

    // Add two sub-drafts
    var subDraft1PublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    })).Value;

    await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 1
    });

    // Activate the first sub-draft via trivia assignment
    await Sender.Send(new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraft1PublicId,
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
    });

    return (draftPartPublicId, subDraft1PublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<(string DraftPartPublicId, string SubDraftPublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupPendingSubDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

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
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    });

    var subDraftPublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    })).Value;

    // Sub-draft remains Pending (no trivia assignment)
    return (draftPartPublicId, subDraftPublicId, drafter1PublicId, drafter2PublicId);
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
    });
    return result.Value;
  }

  private async Task<string> CreateSpeedDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.SpeedDraft.Value,
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
