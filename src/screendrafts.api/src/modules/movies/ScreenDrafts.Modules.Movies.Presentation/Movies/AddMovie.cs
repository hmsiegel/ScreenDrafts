using Microsoft.AspNetCore.Builder;

namespace ScreenDrafts.Modules.Movies.Presentation.Movies;

internal sealed class AddMovie(ISender sender) : Endpoint<AddMovieRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/movies");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Movies)
      .WithDescription("Add a new movie")
      .WithName(nameof(AddMovie));
    });
    Policies(Presentation.Permissions.AddMovie);
  }

  public override async Task HandleAsync(AddMovieRequest req, CancellationToken ct)
  {
    var command = new AddMovieCommand(
      req.ImdbId,
      req.Title,
      req.Year,
      req.Plot,
      req.Image,
      req.ReleaseDate,
      new Uri(req.YoutubeTrailer),
      req.Genres,
      req.Directors,
      req.Actors,
      req.Writers,
      req.Producers,
      req.ProductionCompanies);

    var movieId = await _sender.Send(command, ct);

    await Send.OkAsync(movieId.Value, ct);
  }
}

public sealed record AddMovieRequest(
  string ImdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string ReleaseDate,
  string YoutubeTrailer,
  IReadOnlyCollection<string> Genres,
  IReadOnlyCollection<PersonRequest> Actors,
  IReadOnlyCollection<PersonRequest> Directors,
  IReadOnlyCollection<PersonRequest> Writers,
  IReadOnlyCollection<PersonRequest> Producers,
  IReadOnlyCollection<ProductionCompanyRequest> ProductionCompanies);

internal sealed class AddMovieSummary : Summary<AddMovie>
{
  public AddMovieSummary()
  {
    Summary = "Add a new movie";
    Description = "Add a new movie to the database";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the newly created movie");
    Response(StatusCodes.Status400BadRequest, "Invalid request data");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized request");
    Response(StatusCodes.Status403Forbidden, "Forbidden request");
  }
}
