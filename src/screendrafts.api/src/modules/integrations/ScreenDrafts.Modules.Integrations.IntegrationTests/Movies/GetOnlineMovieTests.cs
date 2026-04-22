using ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMedia;

namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Movies;

public sealed class GetOnlineMovieTests(IntegrationsIntegrationTestWebAppFactory factory)
  : IntegrationsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetOnlineMovie_WhenMovieDetailsNotFound_ShouldReturnNotFoundErrorAsync()
  {
    // Arrange — TMDB returns no details for this TmdbId
    FakeTmdbService.SetDetails(null);

    var command = new GetOnlineMediaCommand { MediaType = MediaType.Movie, TmdbId = 9999 };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.NotFound(9999));
  }

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetOnlineMovie_WithValidTmdbId_ShouldReturnMovieResponseAsync()
  {
    // Arrange
    FakeTmdbService.SetDetails(new TmdbMediaDetails
    {
      Id = 603,
      Title = "The Matrix",
      Overview = "A computer hacker learns the truth about his reality.",
      PosterPath = "/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg",
      ReleaseDate = "1999-03-31",
      Credits = new TmdbCredits { Cast = [], Crew = [] }
    });

    var command = new GetOnlineMediaCommand { MediaType = MediaType.Movie, TmdbId = 603 };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TmdbId.Should().Be(603);
    result.Value.Title.Should().Be("The Matrix");
    result.Value.Year.Should().Be("1999");
    result.Value.ReleaseDate.Should().Be("1999-03-31");
    result.Value.Image.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetOnlineMovie_WithCastAndCrew_ShouldMapDirectorsAndActorsAsync()
  {
    // Arrange
    FakeTmdbService.SetDetails(new TmdbMediaDetails
    {
      Id = 603,
      Title = "The Matrix",
      Overview = "A hacker discovers reality.",
      PosterPath = "/poster.jpg",
      ReleaseDate = "1999-03-31",
      Credits = new TmdbCredits
      {
        Cast =
        [
          new TmdbCastMember { TmdbId = 6384, Name = "Keanu Reeves" },
          new TmdbCastMember { TmdbId = 2975, Name = "Laurence Fishburne" }
        ],
        Crew =
        [
          new TmdbCrewMember { TmdbId = 905152, Name = "Lilly Wachowski", Job = "Director" },
          new TmdbCrewMember { TmdbId = 20629, Name = "Lana Wachowski", Job = "Director" },
          new TmdbCrewMember { TmdbId = 9777, Name = "Joel Silver", Job = "Producer" },
          new TmdbCrewMember { TmdbId = 11111, Name = "Some Writer", Job = "Writer" }
        ]
      }
    });

    var command = new GetOnlineMediaCommand { MediaType = MediaType.Movie, TmdbId = 603 };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Actors.Should().HaveCount(2);
    result.Value.Directors.Should().HaveCount(2);
    result.Value.Directors.Should().Contain(d => d.Name == "Lilly Wachowski");
    result.Value.Directors.Should().Contain(d => d.Name == "Lana Wachowski");
    result.Value.Producers.Should().HaveCount(1);
    result.Value.Producers.Should().Contain(p => p.Name == "Joel Silver");
    result.Value.Writers.Should().HaveCount(1);
    result.Value.Writers.Should().Contain(w => w.Name == "Some Writer");
  }

  [Fact]
  public async Task GetOnlineMovie_WithNullCredits_ShouldReturnEmptyCollectionsAsync()
  {
    // Arrange
    FakeTmdbService.SetDetails(new TmdbMediaDetails
    {
      Id = 9998,
      Title = "No Credits Film",
      Overview = "A film with no credits data.",
      PosterPath = "/nocredits.jpg",
      ReleaseDate = "2020-01-01",
      Credits = new TmdbCredits { Cast = [], Crew = [] }
    });

    var command = new GetOnlineMediaCommand { MediaType = MediaType.Movie, TmdbId = 9998 };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Actors.Should().BeEmpty();
    result.Value.Directors.Should().BeEmpty();
    result.Value.Writers.Should().BeEmpty();
    result.Value.Producers.Should().BeEmpty();
  }
}
