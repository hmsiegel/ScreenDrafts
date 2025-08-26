namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Commands;

public sealed class StartDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task StartDraft_WithValidDraftId_ShouldStartDraftAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    // Act
    var command = new StartDraftCommand(draftId.Value);
    var result = await Sender.Send(command);

    var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
    updatedDraft.Should().NotBeNull();
    updatedDraft.Value.DraftStatus.Should().Be(DraftStatus.InProgress);
  }

  [Fact]
  public async Task StartDraft_WithInvalidDraftId_ShouldNotStartDraftAsync()
  {
    // Arrange
    var draftId = DraftId.Create(Guid.NewGuid()).Value;
    // Act
    var command = new StartDraftCommand(draftId);
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.NotFound(draftId));
  }

  [Fact]
  public async Task StartDraft_WithDraftAlreadyStarted_ShouldNotStartDraftAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    var startDraftCommand = new StartDraftCommand(draftId.Value);
    await Sender.Send(startDraftCommand);
    // Act
    var result = await Sender.Send(new StartDraftCommand(draftId.Value));
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.DraftCanOnlyBeStartedIfItIsCreated);
  }

  [Fact]
  public async Task StartDraft_WithDraftAlreadyCompleted_ShouldNotStartDraftAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    var startDraftCommand = new StartDraftCommand(draftId.Value);
    await Sender.Send(startDraftCommand);
    var completeDraftCommand = new CompleteDraftCommand(draftId.Value);
    await Sender.Send(completeDraftCommand);
    // Act
    var result = await Sender.Send(startDraftCommand);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.DraftCanOnlyBeStartedIfItIsCreated);
  }

  [Fact]
  public async Task StartDraft_ShouldFail_WhenNotAllDraftersAreAddedAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    var drafterId = drafters[0].Id;
    await Sender.Send(new RemoveDrafterFromDraftCommand(draftId.Value, drafterId.Value));
    // Act
    var command = new StartDraftCommand(draftId.Value);
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.CannotStartDraftWithoutAllDrafters);
  }

  [Fact]
  public async Task StartDraft_ShouldFail_WhenNotAllHostsAreAddedAsync()
  {
    // Arrange
    var (draftId, _, hosts) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    var hostId = hosts[0].Id;
    await Sender.Send(new RemoveHostFromDraftCommand(draftId.Value, hostId.Value));

    // Act
    var command = new StartDraftCommand(draftId.Value);
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.CannotStartDraftWithoutAllHosts);
  }
}
