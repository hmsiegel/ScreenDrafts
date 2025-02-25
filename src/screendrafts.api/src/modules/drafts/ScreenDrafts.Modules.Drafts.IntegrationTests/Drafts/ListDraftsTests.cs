namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ListDraftsTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task ShoudReturn_AllDrafts_WhenDraftsExistAsync()
  {
    // Arrange
    List<DraftResponse> drafts = [];
    do
    {
      var draft = DraftFactory.CreateStandardDraft().Value;
      var draftId = await Sender.Send(new CreateDraftCommand(
        draft.Title.Value,
        draft.DraftType,
        draft.TotalPicks,
        draft.TotalDrafters,
        draft.TotalHosts,
        draft.EpisodeType,
        draft.DraftStatus));

      var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value)); 

      drafts.Add(updatedDraft.Value);

    } while (drafts.Count < 20);

    // Act
    var allDrafts = await Sender.Send(new ListDraftsQuery());

    allDrafts.IsSuccess.Should().BeTrue();

    var allDraftsList = allDrafts.Value.ToList();

    // Assert
    allDraftsList.Count.Should().Be(20);
    allDraftsList.Should().BeEquivalentTo(drafts);
  }

  [Fact]
  public async Task ShouldReturn_EmptyList_WhenNoDraftsExistAsync()
  {
    // Act
    var allDrafts = await Sender.Send(new ListDraftsQuery());
    // Assert
    allDrafts.IsSuccess.Should().BeTrue();
    allDrafts.Value.Should().BeEmpty();
  }
}
