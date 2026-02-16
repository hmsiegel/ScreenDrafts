using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

internal sealed class CommandHandler(
  IImdbService imdbService,
  IOmdbService omdbService) 
  : ICommandHandler<Command, Response>
{
  private readonly IImdbService _imdbService = imdbService;
  private readonly IOmdbService _omdbService = omdbService;

  public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
  {
    var movieData = await _imdbService.GetMovieInformation(command.ImdbId, TitleOptions.Trailer);
    var fullCast = await _imdbService.GetFullCast(command.ImdbId);

    if (!string.IsNullOrEmpty(movieData.Title))
    {
      if (movieData.ErrorMessage.Contains("404", StringComparison.InvariantCultureIgnoreCase))
      {
        return Result.Failure<Response>(MovieErrors.NotFound(command.ImdbId));
      }

      Uri trailerUri = null!;
      if (movieData.Trailer != null && !string.IsNullOrEmpty(movieData.Trailer.Link))
      {
        trailerUri = new Uri(movieData.Trailer.LinkEmbed);
      }

      var producerList = fullCast.Others?.Where(cast => cast.Job == "Produced by").ToList()
        ?? [];
      var producers = producerList.
        SelectMany(x => x.Items.
          Select(x => new ProducerModel(x.Name, x.Id))).ToList()
        ?? [];

      var genres = movieData.Genres.Trim().Split(", ").ToList();

      var actors =
        movieData.ActorList.Count > 0
          ? [.. movieData.ActorList.Select(actor => new ActorModel(actor.Name, actor.Id))]
          : new List<ActorModel>();

      var directors =
        movieData.DirectorList.Count > 0
          ? [.. movieData.DirectorList.Select(director => new DirectorModel(director.Name, director.Id))]
          : new List<DirectorModel>();

      var writers =
        movieData.WriterList.Count > 0
          ? [.. movieData.WriterList.Select(writer => new WriterModel(writer.Name, writer.Id))]
          : new List<WriterModel>();

      var productionCompanies =
        movieData.CompanyList.Count > 0
          ? [.. movieData.CompanyList
          .GroupBy(company => company.Id)
          .Select(group => group.First())
          .Select(company => new ProductionCompanyModel(company.Id, company.Name))]
          : new List<ProductionCompanyModel>();

      var response = new Response(
        command.ImdbId,
        movieData.Title,
        movieData.Year,
        movieData.Plot,
        movieData.Image,
        movieData.ReleaseDate,
        trailerUri,
        genres,
        actors,
        directors,
        writers,
        producers,
        productionCompanies);

      return response;
    }
    else
    {
      var response = await SearchOmdbAsync(command);

      return response;
    }
  }
  private async Task<Result<Response>> SearchOmdbAsync(Command command)
  {
    var omdbData = await _omdbService.GetItemByIdAsync(command.ImdbId, true);

    if (omdbData.Title is null)
    {
      return Result.Failure<Response>(MovieErrors.NotFound(command.ImdbId));
    }

    var genres = omdbData.Genre.Trim().Split(", ").ToList();

    var response = new Response(
      command.ImdbId,
      omdbData.Title,
      omdbData.Year,
      omdbData.Plot,
      omdbData.Poster,
      omdbData.Released,
      null,
      genres,
      null!,
      null!,
      null!,
      null!,
      null!);

    return response;
  }
}
