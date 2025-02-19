namespace ScreenDrafts.Modules.Movies.Presentation.Movies;

internal sealed class AddMovie(ISender sender) : Endpoint<AddMovieRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/movies");
    Description(x => x.WithTags(Presentation.Tags.Movies));
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

    await SendOkAsync(movieId.Value, ct);
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
