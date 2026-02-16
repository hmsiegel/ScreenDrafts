using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using ScreenDrafts.Common.Presentation.Http;
using ScreenDrafts.Common.Presentation.Results;
using ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;

internal sealed class Endpoint(ISender sender) : ScreenDraftsEndpoint<Request, Response>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get(MovieRoutes.Search);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Movies)
      .WithName(IntegrationsOpenApi.Names.Movies_Search)
      .Produces<Response>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(IntegrationsOpenApi.Permissions.MoviesSearch);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Command(req.ImdbId);

    var result = await _sender.Send(query, ct);

    await this.SendOkAsync<Response>(result.Value, ct);
  }
}

internal sealed record Request(string ImdbId);
