namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public class DrafterQueryTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetDrafter_WithValidId_ShouldReturnDrafterAsync()
  {
    // Arrange
    var drafter = await CreateDrafterAsync();

    // Act
    var result = await Sender.Send(new GetDrafterQuery(drafter));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(drafter);
    result.Value.DisplayName.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetDrafter_WithInvalidId_ShouldReturnErrorAsync()
  {
    // Arrange
    var invalidDrafterId = Guid.NewGuid();

    // Act
    var result = await Sender.Send(new GetDrafterQuery(invalidDrafterId));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(invalidDrafterId));
  }

  [Fact]
  public async Task GetVetoOverride_WithValidId_ShouldReturnVetoOverrideAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.MiniMega);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickResult = await Sender.Send(new AddPickCommand(
      reloadedDraftResult.Value.Id,
      7,
      movieId.Value,
      1,
      drafters[0].Id.Value,
      null));

    var pickId = pickResult.Value;

    var vetoResult = await Sender.Send(new ExecuteVetoCommand(
      null,
      drafters[1].Id.Value,
      pickId,
      reloadedDraftResult.Value.Id));

    var vetoId = vetoResult.Value;

    var overrideResult = await Sender.Send(new ExecuteVetoOverrideCommand(
      drafters[2].Id.Value,
      null,
      vetoId));

    var overrideId = overrideResult.Value;

    // Act
    var result = await Sender.Send(new GetVetoOverrideQuery(overrideId));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(overrideId);
    result.Value.Veto.Id.Should().Be(vetoId);
  }

  private async Task<Guid> CreateDrafterAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateDrafterCommand(personId);
    var result = await Sender.Send(command);
    return result.Value;
  }
}
