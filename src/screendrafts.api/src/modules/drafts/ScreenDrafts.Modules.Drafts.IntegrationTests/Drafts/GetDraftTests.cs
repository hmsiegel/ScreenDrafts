namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class GetDraftTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnError_WhenDraftDoesNotExistAsync()
  {
    // Arrange
    var draftId = Guid.NewGuid();
    // Act
    Result<DraftResponse> draftResult = await Sender.Send(new GetDraftQuery(draftId));
    // Assert
    draftResult.Errors[0].Should().Be(DraftErrors.NotFound(draftId));
  }

  [Fact]
  public async Task Should_ReturnDraft_WhenDraftExistsAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));

    // Act
    Result<DraftResponse> draftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    // Assert
    draftResult.IsSuccess.Should().BeTrue();
    draftResult.Value.Should().NotBeNull();
  }
}
