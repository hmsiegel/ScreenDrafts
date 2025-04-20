namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class UpdateReleaseDateTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task UpdateReleaseDate_WhenDraftExists_ShouldUpdateReleaseDateAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    var releaseDate = DraftReleaseDate.Create(DraftId.Create(draftId.Value), Faker.Date.PastDateOnly());

    // Act
    var result = await Sender.Send(new UpdateReleaseDateCommand(releaseDate.DraftId.Value, releaseDate.ReleaseDate));

    // Assert
    result.IsSuccess.Should().BeTrue();
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));
    var draftResponse = draft.Value;

    draftResponse.ReleaseDates.Should().Contain(new ReleaseDateResponse(releaseDate.ReleaseDate));
  }

  [Fact]
  public async Task UpdateReleaseDate_WhenDraftDoesNotExist_ShouldReturnFailureAsync()
  {
    // Arrange
    var draftId = DraftId.Create(Faker.Random.Guid());
    var releaseDate = DraftReleaseDate.Create(draftId, Faker.Date.PastDateOnly());
    // Act
    var result = await Sender.Send(new UpdateReleaseDateCommand(draftId.Value, releaseDate.ReleaseDate));
    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
