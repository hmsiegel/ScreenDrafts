using ScreenDrafts.Modules.Integrations.Features.Movies.FetchMedia;

namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Movies;

/// <summary>
/// FetchMediaCommandHandler had no coverage at all before this file. Its failure
/// branch unconditionally read command.TmdbId!.Value to build a NotFound error --
/// TmdbId is null for video games, music videos, short films, and Imdb-only movie
/// lookups, so any failed fetch through one of those paths threw a
/// NullReferenceException-shaped crash instead of returning a clean Result.Failure.
/// Fixed to propagate GetOnlineMediaCommandHandler's own (correctly-scoped) error
/// instead of re-deriving a new one. These tests cover the happy path and the
/// regression.
/// </summary>
public sealed class FetchMediaTests(IntegrationsIntegrationTestWebAppFactory factory)
  : IntegrationsIntegrationTest(factory)
{
  [Fact]
  public async Task FetchMedia_ForAMovieWithValidTmdbId_ShouldSucceedAsync()
  {
    // Arrange
    FakeTmdbService.SetDetails(
      new TmdbMediaDetails
      {
        Id = 603,
        Title = "The Matrix",
        Overview = "A computer hacker learns the truth about his reality.",
        PosterPath = "/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg",
        ReleaseDate = "1999-03-31",
        Credits = new TmdbCredits { Cast = [], Crew = [] },
      }
    );
    var command = new FetchMediaCommand { MediaType = MediaType.Movie, TmdbId = 603 };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task FetchMedia_ForAMovieByImdbIdThatIsNotFound_ShouldFailCleanlyRatherThanThrowAsync()
  {
    // Arrange -- regression: no TmdbId on this command at all (Imdb-only lookup),
    // and OMDb has nothing for this id (FakeOmdbService defaults to null).
    var command = new FetchMediaCommand
    {
      MediaType = MediaType.Movie,
      TmdbId = null,
      ImdbId = "tt9999999",
    };

    // Act
    var act = async () => await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var result = await act.Should().NotThrowAsync();
    result.Subject.IsFailure.Should().BeTrue();
    result.Subject.Errors.Should().Contain(e => e.Code == MovieErrors.NotFound("tt9999999").Code);
  }

  [Fact]
  public async Task FetchMedia_WhenMediaTypeIsUnsupported_ShouldFailCleanlyRatherThanThrowAsync()
  {
    // Arrange -- another TmdbId-less failure path: GetOnlineMediaCommandHandler
    // falls through to MovieErrors.UnsupportedMediaType for a media type its switch
    // doesn't recognize, e.g. TvEpisode with none of the required episode fields.
    var command = new FetchMediaCommand
    {
      MediaType = MediaType.TvEpisode,
      TmdbId = null,
      TvSeriesTmdbId = null,
      SeasonNumber = null,
      EpisodeNumber = null,
    };

    // Act
    var act = async () => await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var result = await act.Should().NotThrowAsync();
    result.Subject.IsFailure.Should().BeTrue();
    result.Subject.Errors.Should().Contain(e => e.Code == MovieErrors.EpisodeFieldsAreRequired.Code);
  }
}
