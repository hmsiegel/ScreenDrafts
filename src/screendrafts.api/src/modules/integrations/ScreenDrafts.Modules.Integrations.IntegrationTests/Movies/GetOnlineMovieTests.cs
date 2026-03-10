using ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Movies;

public sealed class GetOnlineMovieTests(IntegrationsIntegrationTestWebAppFactory factory)
  : IntegrationsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetOnlineMovie_WhenImdbIdNotFoundOnTmdb_ShouldReturnNotFoundErrorAsync()
  {
    // Arrange — TMDB returns no result for this IMDb ID
    FakeTmdbService.SetFindResult(null);

    var command = new GetOnlineMovieCommand("tt0000001");

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.NotFound("tt0000001"));
  }

  [Fact]
  public async Task GetOnlineMovie_WhenMovieDetailsNotFound_ShouldReturnNotFoundErrorAsync()
  {
    // Arrange — TMDB finds the movie but details are unavailable
    FakeTmdbService.SetFindResult(new TmdbMovieSearchResult { Id = 603, Title = "The Matrix" });
    FakeTmdbService.SetDetails(null);

    var command = new GetOnlineMovieCommand("tt0133093");

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.NotFound("tt0133093"));
  }

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetOnlineMovie_WithValidImdbId_ShouldReturnMovieResponseAsync()
  {
    // Arrange
    FakeTmdbService.SetFindResult(new TmdbMovieSearchResult
    {
      Id = 603,
      Title = "The Matrix",
      ReleaseDate = "1999-03-31",
      Overview = "A computer hacker learns the truth about his reality.",
      PosterPath = "/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg"
    });

    FakeTmdbService.SetDetails(new TmdbMovieDetails
    {
      Id = 603,
      Title = "The Matrix",
      Overview = "A computer hacker learns the truth about his reality.",
      PosterPath = "/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg",
      ReleaseDate = "1999-03-31",
      Credits = new TmdbCredits
      {
        Cast =
        [
          new TmdbCastMember { Id = 6384, Name = "Keanu Reeves" },
          new TmdbCastMember { Id = 2975, Name = "Laurence Fishburne" }
        ],
        Crew =
        [
          new TmdbCrewMember { Id = 905152, Name = "Lilly Wachowski", Job = "Director" },
          new TmdbCrewMember { Id = 20629, Name = "Lana Wachowski", Job = "Director" },
          new TmdbCrewMember { Id = 9777, Name = "Joel Silver", Job = "Producer" }
        ]
      }
    });

    var command = new GetOnlineMovieCommand("tt0133093");

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.ImdbId.Should().Be("tt0133093");
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
    FakeTmdbService.SetFindResult(new TmdbMovieSearchResult { Id = 603, Title = "The Matrix" });

    FakeTmdbService.SetDetails(new TmdbMovieDetails
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
          new TmdbCastMember { Id = 6384, Name = "Keanu Reeves" },
          new TmdbCastMember { Id = 2975, Name = "Laurence Fishburne" }
        ],
        Crew =
        [
          new TmdbCrewMember { Id = 905152, Name = "Lilly Wachowski", Job = "Director" },
          new TmdbCrewMember { Id = 20629, Name = "Lana Wachowski", Job = "Director" },
          new TmdbCrewMember { Id = 9777, Name = "Joel Silver", Job = "Producer" },
          new TmdbCrewMember { Id = 11111, Name = "Some Writer", Job = "Writer" }
        ]
      }
    });

    var command = new GetOnlineMovieCommand("tt0133093");

    // Act
    var result = await Sender.Send(command);

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
    FakeTmdbService.SetFindResult(new TmdbMovieSearchResult { Id = 9999, Title = "No Credits Film" });

    FakeTmdbService.SetDetails(new TmdbMovieDetails
    {
      Id = 9999,
      Title = "No Credits Film",
      Overview = "A film with no credits data.",
      PosterPath = "/nocredits.jpg",
      ReleaseDate = "2020-01-01",
      Credits = new TmdbCredits { Cast = [], Crew = [] }
    });

    var command = new GetOnlineMovieCommand("tt9999999");

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Actors.Should().BeEmpty();
    result.Value.Directors.Should().BeEmpty();
    result.Value.Writers.Should().BeEmpty();
    result.Value.Producers.Should().BeEmpty();
  }
}
