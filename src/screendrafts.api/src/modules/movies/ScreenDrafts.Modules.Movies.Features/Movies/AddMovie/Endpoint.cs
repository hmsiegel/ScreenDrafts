using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using ScreenDrafts.Common.Abstractions.Results;
using ScreenDrafts.Common.Presentation.Http;
using ScreenDrafts.Common.Presentation.Responses;
using ScreenDrafts.Common.Presentation.Results;

namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, Guid>
{
  public override void Configure()
  {
    Post(MoviesRoutes.Base);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Movies)
      .WithName(MoviesOpenApi.Names.Movies_AddMovie)
      .Produces<Guid>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(MoviesAuth.Permissions.MoviesAdd);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command(
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

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => MovieLocations.ById(created.PublicId),
      ct);
  }
}
