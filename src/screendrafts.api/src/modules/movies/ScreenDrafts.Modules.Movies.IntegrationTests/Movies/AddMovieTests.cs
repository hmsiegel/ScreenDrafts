namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

public sealed class AddMovieTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenDataIsValidAsync()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;

    List<Genre> genres = [];
    for (int i = 0; i < 3; i++)
    {
      genres.Add(MovieFactory.CreateGenre().Value);
    }

    List<Domain.Movies.Person> actors = [];
    for (int i = 0; i < 3; i++)
    {
      actors.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Movies.Person> directors = [];
    for (int i = 0; i < 1; i++)
    {
      directors.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Movies.Person> writers = [];
    for (int i = 0; i < 1; i++)
    {
      writers.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Movies.Person> producers = [];
    for (int i = 0; i < 4; i++)
    {
      producers.Add(MovieFactory.CreatePerson().Value);
    }

    List<ProductionCompany> productionCompanies = [];
    for (int i = 0; i < 2; i++)
    {
      productionCompanies.Add(MovieFactory.CreateProductionCompany().Value);
    }

    var request = new AddMovieCommand(
      movie.ImdbId,
      movie.Title,
      movie.Year,
      movie.Plot!,
      movie.Image,
      movie.ReleaseDate,
      movie.YoutubeTrailerUrl,
      [.. genres.Select(x => x.Name)],
      [.. actors.Select(x => new PersonRequest(x.Name, x.ImdbId))],
      [.. directors.Select(x => new PersonRequest(x.Name, x.ImdbId))],
      [.. writers.Select(x => new PersonRequest(x.Name, x.ImdbId))],
      [.. producers.Select(x => new PersonRequest(x.Name, x.ImdbId))],
      [.. productionCompanies.Select(x => new ProductionCompanyRequest(x.Name, x.ImdbId))]);


    // Act
    var result = await Sender.Send(request);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
  }
}
