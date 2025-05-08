﻿namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ListUpcomingDraftsTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
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
        draft.TotalDrafters,
        draft.TotalDrafterTeams,
        draft.TotalHosts,
        draft.EpisodeType,
        draft.DraftStatus));

      if (!draftId.IsSuccess)
      {
        continue;
      }

      var draftReleaseDate = DraftReleaseDate.Create(DraftId.Create(draftId.Value), Faker.Date.FutureDateOnly());

      var updatedReleaseDate =
        await Sender.Send(new UpdateReleaseDateCommand(draftReleaseDate.DraftId.Value, draftReleaseDate.ReleaseDate));

      if (!updatedReleaseDate.IsSuccess)
      {
        continue;
      }

      updatedReleaseDate.IsSuccess.Should().BeTrue();

      var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));

      if (updatedDraft.IsSuccess && updatedDraft.Value is not null)
      {
        drafts.Add(updatedDraft.Value);
      }
    } while (drafts.Count < 20);

    // Act
    var query = new ListUpcomingDraftsQuery();
    var response = await Sender.Send(query);

    response.Should().NotBeNull();

    var upcomingDrafts = response.Value.ToList();

    // Assert
    upcomingDrafts.Count.Should().Be(20);

    var upcomingReleaseDates = upcomingDrafts
      .Select(d => d.ReleaseDates!)
      .SelectMany(r => r)
      .Select(r => r.ReleaseDate)
      .ToList();

    upcomingReleaseDates.Should().BeInAscendingOrder();
  }

  [Fact]
  public async Task ListUpcomingDrafts_ShouldReturnEmptyList_WhenNoUpcomingDraftsExistAsync()
  {
    // Act
    var query = new ListUpcomingDraftsQuery();
    var response = await Sender.Send(query);
    // Assert
    response.IsSuccess.Should().BeTrue();
    response.Value.Should().BeEmpty();
  }
}
