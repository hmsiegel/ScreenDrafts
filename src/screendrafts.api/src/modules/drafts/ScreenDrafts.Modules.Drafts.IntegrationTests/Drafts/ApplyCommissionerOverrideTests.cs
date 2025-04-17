namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ApplyCommissionerOverrideTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task ApplyCommissionerOverride_WhenValidPick_ShouldApplyCommissionerOverrideAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));

    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      draftId.Value,
      7,
      movieId.Value,
      1,
      drafter.Id.Value, null));

    pickId.IsSuccess.Should().BeTrue();

    var command = new ApplyCommissionerOverrideCommand(pickId.Value);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var commissionerOverride = await Sender.Send(new GetCommissionerOverridesByDraftQuery(draft.Value.Id));

    commissionerOverride.Value.Should().NotBeNull();
    commissionerOverride.Value.Should().HaveCount(1);
    commissionerOverride.Value[0].PickId.Should().Be(pickId.Value);
    commissionerOverride.Value[0].DrafterId.Should().Be(drafter.Id.Value);
    commissionerOverride.Value[0].MovieId.Should().Be(movieId.Value);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_WhenInvalidPick_ShouldNotApplyCommissionerOverrideAsync()
  {
    // Arrange
    var pickId = Guid.NewGuid();
    var command = new ApplyCommissionerOverrideCommand(pickId);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().HaveCount(1);
    result.Errors[0].Should().Be(PickErrors.NotFound(pickId));
  }
}
