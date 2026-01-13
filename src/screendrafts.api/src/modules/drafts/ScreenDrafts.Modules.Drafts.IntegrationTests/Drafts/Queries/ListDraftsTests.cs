namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Queries;

public sealed class ListDraftsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
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
        draft.TotalParticipants,
        draft.TotalDrafterTeams,
        draft.TotalHosts,
        draft.DraftStatus));


      var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));

      drafts.Add(updatedDraft.Value);

    } while (drafts.Count < 20);

    var isPatreonOnly = true;

    // Act
    var query = new ListDraftsQuery(Page: 1, PageSize: 20, IsPatreonOnly: isPatreonOnly);
    var allDrafts = await Sender.Send(query);

    allDrafts.IsSuccess.Should().BeTrue();

    var allDraftsList = allDrafts.Value.Items.ToList();


    // Assert
    allDraftsList.Count.Should().Be(20);
    allDraftsList.Should().BeEquivalentTo(drafts);
  }

  [Fact]
  public async Task ShouldReturn_EmptyList_WhenNoDraftsExistAsync()
  {
    var isPatreonOnly = true;
    // Act
    var query = new ListDraftsQuery(Page: 1, PageSize: 20, IsPatreonOnly: isPatreonOnly);
    var allDrafts = await Sender.Send(query);
    // Assert
    var allDraftsList = allDrafts.Value.Items.ToList();
    allDrafts.IsSuccess.Should().BeTrue();
    allDraftsList.Should().BeEmpty();
  }
}
