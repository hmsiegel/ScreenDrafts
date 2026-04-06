namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class SetSubDraftSubjectTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SetSubDraftSubject_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId) = await SetupSpeedDraftPartWithSubDraftAsync();

    var command = new SetSubDraftSubjectCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      SubjectKind = SubjectKind.Actor.Value,
      SubjectName = "Tom Hanks"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetSubDraftSubject_ShouldPersistSubjectInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId) = await SetupSpeedDraftPartWithSubDraftAsync();

    var command = new SetSubDraftSubjectCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      SubjectKind = SubjectKind.Director.Value,
      SubjectName = "Christopher Nolan"
    };

    // Act
    await Sender.Send(command);

    // Assert
    var draftPart = await DbContext.DraftParts
      .Include("_subDrafts")
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == draftPartPublicId);

    var subDraft = draftPart.SubDrafts.First(s => s.PublicId == subDraftPublicId);
    subDraft.SubjectKind.Should().Be(SubjectKind.Director);
    subDraft.SubjectName.Should().Be("Christopher Nolan");
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SetSubDraftSubject_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new SetSubDraftSubjectCommand
    {
      DraftPartPublicId = Faker.Random.AlphaNumeric(10),
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}",
      SubjectKind = SubjectKind.Actor.Value,
      SubjectName = "Test Actor"
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
  public async Task SetSubDraftSubject_WithNonExistentSubDraft_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _) = await SetupSpeedDraftPartWithSubDraftAsync();

    var command = new SetSubDraftSubjectCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}",
      SubjectKind = SubjectKind.Actor.Value,
      SubjectName = "Test Actor"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — empty subject name
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SetSubDraftSubject_WithEmptySubjectName_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId) = await SetupSpeedDraftPartWithSubDraftAsync();

    var command = new SetSubDraftSubjectCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      SubjectKind = SubjectKind.Actor.Value,
      SubjectName = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string SubDraftPublicId)> SetupSpeedDraftPartWithSubDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

    var subDraftPublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    })).Value;

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
