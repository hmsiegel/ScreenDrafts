namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.Zoom;

public sealed class EndZoomSessionTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task EndZoomSession_ShouldReturnSuccess_WhenSessionIsActiveAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartWithSessionAsync();

    // Act
    var result = await Sender.Send(
      new EndZoomSessionCommand { DraftPartPublicId = draftPartPublicId },
      TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task EndZoomSession_ShouldClearSessionName_WhenSessionIsActiveAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartWithSessionAsync();

    // Act
    await Sender.Send(
      new EndZoomSessionCommand { DraftPartPublicId = draftPartPublicId },
      TestContext.Current.CancellationToken);

    // Assert
    var draftPart = await DbContext.DraftParts
      .FirstOrDefaultAsync(dp => dp.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

    draftPart!.ZoomSessionName.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task EndZoomSession_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Arrange
    var command = new EndZoomSessionCommand
    {
      DraftPartPublicId = $"dp_{Faker.Random.AlphaNumeric(15)}"
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — no active session
  // -------------------------------------------------------------------------

  [Fact]
  public async Task EndZoomSession_ShouldFail_WhenNoActiveSessionAsync()
  {
    // Arrange — draft part exists but has no active session
    var draftPartPublicId = await CreateDraftPartAsync();

    // Act
    var result = await Sender.Send(
      new EndZoomSessionCommand { DraftPartPublicId = draftPartPublicId },
      TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.NoActiveZoomSession);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<string> SetupDraftPartWithSessionAsync()
  {
    var draftPartPublicId = await CreateDraftPartAsync();
    var hostPublicId = await new HostFactory(Sender, Faker).CreateAndSaveHostAsync();

    await Sender.Send(new StartZoomSessionCommand
    {
      DraftPartPublicId = draftPartPublicId,
      HostPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    return draftPartPublicId;
  }

  private async Task<string> CreateDraftPartAsync()
  {
    var seriesResult = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);

    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesResult.Value,
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftResult.Value,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    }, TestContext.Current.CancellationToken);

    return await GetFirstDraftPartPublicIdAsync(draftResult.Value);
  }
}
