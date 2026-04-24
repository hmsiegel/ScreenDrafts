namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.Zoom;

public sealed class StartZoomSessionTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task StartZoomSession_ShouldReturnSessionNameAndToken_WhenSuccessfulAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartAsync();
    var hostPublicId = await new HostFactory(Sender, Faker).CreateAndSaveHostAsync();

    var command = new StartZoomSessionCommand
    {
      DraftPartPublicId = draftPartPublicId,
      HostPublicId = hostPublicId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.SessionName.Should().Be($"screendrafts-{draftPartPublicId}");
    result.Value.Token.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task StartZoomSession_ShouldPersistSessionName_WhenSuccessfulAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartAsync();
    var hostPublicId = await new HostFactory(Sender, Faker).CreateAndSaveHostAsync();

    // Act
    await Sender.Send(new StartZoomSessionCommand
    {
      DraftPartPublicId = draftPartPublicId,
      HostPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    // Assert
    var draftPart = await DbContext.DraftParts
      .FirstOrDefaultAsync(dp => dp.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

    draftPart!.ZoomSessionName.Should().Be($"screendrafts-{draftPartPublicId}");
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task StartZoomSession_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Arrange
    var command = new StartZoomSessionCommand
    {
      DraftPartPublicId = $"dp_{Faker.Random.AlphaNumeric(15)}",
      HostPublicId = $"h_{Faker.Random.AlphaNumeric(15)}"
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — session already active
  // -------------------------------------------------------------------------

  [Fact]
  public async Task StartZoomSession_ShouldFail_WhenSessionAlreadyActiveAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartAsync();
    var hostPublicId = await new HostFactory(Sender, Faker).CreateAndSaveHostAsync();

    var command = new StartZoomSessionCommand
    {
      DraftPartPublicId = draftPartPublicId,
      HostPublicId = hostPublicId
    };

    // First session starts successfully
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — second attempt on the same draft part
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.ZoomSessionAlreadyActive);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

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
