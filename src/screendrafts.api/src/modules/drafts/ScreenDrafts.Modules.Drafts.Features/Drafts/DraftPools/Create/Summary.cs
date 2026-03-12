using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Creates a new draft pool.";
    Description = "Creates a new draft pool.";
    Response(StatusCodes.Status204NoContent, "The draft pool was created successfully.");
    Response(StatusCodes.Status400BadRequest, "The request is invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authorized.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to create a draft pool.");
    Response(StatusCodes.Status404NotFound, "The specified resource was not found.");
  }
}
