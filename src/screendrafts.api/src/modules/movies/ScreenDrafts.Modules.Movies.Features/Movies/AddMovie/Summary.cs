using FastEndpoints;

using Microsoft.AspNetCore.Http;

namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a new movie";
    Description = "Add a new movie to the database";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the newly created movie");
    Response(StatusCodes.Status400BadRequest, "Invalid request data");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized request");
    Response(StatusCodes.Status403Forbidden, "Forbidden request");
  }
}
