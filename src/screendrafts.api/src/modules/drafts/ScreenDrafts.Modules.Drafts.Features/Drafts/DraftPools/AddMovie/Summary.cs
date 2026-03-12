using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a movie to a draft pool.";
    Description = "Add a movie to a draft pool.";
    Response(StatusCodes.Status204NoContent, "Movie added to draft pool.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status404NotFound, "Draft or movie not found.");
  }
}
