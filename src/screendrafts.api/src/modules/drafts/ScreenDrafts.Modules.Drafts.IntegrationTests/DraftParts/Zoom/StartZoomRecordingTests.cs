namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.Zoom;

public sealed class StartZoomRecordingTests : DraftsIntegrationTest
{
  private readonly DraftsIntegrationTestWebAppFactory _factory;

  public StartZoomRecordingTests(DraftsIntegrationTestWebAppFactory factory) : base(factory)
    => _factory = factory;
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task StartZoomRecording_ShouldPublishIntegrationEvent_WhenSessionIsActiveAsync()
  {
    // Arrange
    var (draftPartPublicId, sessionName) = await SetupDraftPartWithSessionAsync();
    _factory.EventBusCapture.Clear();

    var command = new StartZoomRecordingCommand { DraftPartPublicId = draftPartPublicId };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var published = _factory.EventBusCapture.CapturedEvents
      .OfType<StartZoomRecordingRequestedIntegrationEvent>()
      .ToList();

    published.Should().ContainSingle();
    published[0].SessionName.Should().Be(sessionName);
    published[0].DraftPartPublicId.Should().Be(draftPartPublicId);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task StartZoomRecording_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Arrange
    var command = new StartZoomRecordingCommand
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
  public async Task StartZoomRecording_ShouldFail_WhenNoActiveSessionAsync()
  {
    // Arrange — draft part without a running Zoom session
    var draftPartPublicId = await CreateDraftPartAsync();

    var command = new StartZoomRecordingCommand { DraftPartPublicId = draftPartPublicId };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.NoActiveZoomSession);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string SessionName)> SetupDraftPartWithSessionAsync()
  {
    var draftPartPublicId = await CreateDraftPartAsync();
    var hostPublicId = await new HostFactory(Sender, Faker).CreateAndSaveHostAsync();

    var startResult = await Sender.Send(new StartZoomSessionCommand
    {
      DraftPartPublicId = draftPartPublicId,
      HostPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, startResult.Value.SessionName);
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
