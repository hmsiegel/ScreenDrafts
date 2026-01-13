namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Queries;

public sealed class ListUpcomingDraftsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ListUpcomingDrafts_ShouldReturnUpcomingDraftsAsync()
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

      if (!draftId.IsSuccess)
      {
        continue;
      }


      var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));

      if (updatedDraft.IsSuccess && updatedDraft.Value is not null)
      {
        drafts.Add(updatedDraft.Value);
      }
    } while (drafts.Count < 20);

    var isPatreonOnly = true;
    var isAdmin = true;

    var userId = Guid.NewGuid();

    // Act
    var query = new ListUpcomingDraftsQuery(isPatreonOnly, userId, isAdmin);
    var response = await Sender.Send(query);

    response.Should().NotBeNull();

    var upcomingDrafts = response.Value.ToList();

    // Assert
    upcomingDrafts.Count.Should().Be(20);

    var upcomingReleaseDates = upcomingDrafts
      .Select(d => d.ReleaseDates!)
      .SelectMany(r => r)
      .ToList();

    upcomingReleaseDates.Should().BeInAscendingOrder();
  }

  [Fact]
  public async Task ListUpcomingDrafts_ShouldReturnEmptyList_WhenNoUpcomingDraftsExistAsync()
  {
    var isPatreonOnly = true;
    var isAdmin = true;

    var userId = Guid.NewGuid();
    // Act
    var query = new ListUpcomingDraftsQuery(isPatreonOnly, userId, isAdmin);
    var response = await Sender.Send(query);
    // Assert
    response.IsSuccess.Should().BeTrue();
    response.Value.Should().BeEmpty();
  }
}
