namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.Zoom;

public sealed class GetZoomSessionTokenTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetZoomSessionToken_ShouldReturnTokenAndSessionName_WhenSessionIsActiveAsync()
  {
    // Arrange
    var (draftPartPublicId, sessionName) = await SetupDraftPartWithSessionAsync();
    var participantPublicId = $"dr_{Faker.Random.AlphaNumeric(15)}";

    var query = new GetZoomSessionTokenQuery
    {
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = participantPublicId
    };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.SessionName.Should().Be(sessionName);
    result.Value.Token.Should().NotBeNullOrEmpty();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetZoomSessionToken_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Arrange
    var query = new GetZoomSessionTokenQuery
    {
      DraftPartPublicId = $"dp_{Faker.Random.AlphaNumeric(15)}",
      ParticipantPublicId = $"dr_{Faker.Random.AlphaNumeric(15)}"
    };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — no active session
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetZoomSessionToken_ShouldFail_WhenNoActiveSessionAsync()
  {
    // Arrange — draft part exists but session was never started
    var draftPartPublicId = await CreateDraftPartAsync();

    var query = new GetZoomSessionTokenQuery
    {
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = $"dr_{Faker.Random.AlphaNumeric(15)}"
    };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

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
