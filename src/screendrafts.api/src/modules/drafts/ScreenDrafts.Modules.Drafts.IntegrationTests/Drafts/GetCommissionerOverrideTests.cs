namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class GetCommissionerOverrideTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task GetCommissionerOverridesByDraft_WithValidDraft_ShouldReturnOverridesAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));

    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      DraftId: draftId.Value,
      DrafterTeamId: null,
      Position: 7,
      MovieId: movieId.Value,
      DrafterId: drafter.Id.Value, PlayOrder: 1));

    pickId.IsSuccess.Should().BeTrue();

    var command = new ApplyCommissionerOverrideCommand(pickId.Value);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var commissionerOverrides = await Sender.Send(new GetCommissionerOverridesByDraftQuery(draftId.Value));

    commissionerOverrides.IsSuccess.Should().BeTrue();
    commissionerOverrides.Value.Should().NotBeNull();
    commissionerOverrides.Value.Should().HaveCount(1);
    commissionerOverrides.Value[0].PickId.Should().Be(pickId.Value);
  }

  [Fact]
  public async Task GetCommissionerOverridesByDraft_WithInvalidDraft_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));

    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      DraftId: draftId.Value,
      DrafterTeamId: null,
      Position: 7,
      MovieId: movieId.Value,
      DrafterId: drafter.Id.Value, PlayOrder: 1));

    pickId.IsSuccess.Should().BeTrue();

    var command = new ApplyCommissionerOverrideCommand(pickId.Value);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var commissionerOverrides = await Sender.Send(new GetCommissionerOverridesByDraftQuery(Guid.NewGuid()));

    commissionerOverrides.IsFailure.Should().BeTrue();
  }
}
