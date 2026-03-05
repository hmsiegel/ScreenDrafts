using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVeto;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Apply a veto to a draft pick.";
    Description = "Apply a veto to a draft pick.";
    Response(StatusCodes.Status204NoContent, "The veto was applied successfully.");
    Response(StatusCodes.Status404NotFound, "The specified draft pick was not found.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authorized to apply a veto.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to apply a veto.");
  }
}
