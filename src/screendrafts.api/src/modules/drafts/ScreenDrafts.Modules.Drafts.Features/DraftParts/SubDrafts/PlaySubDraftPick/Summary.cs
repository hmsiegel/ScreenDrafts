using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.PlaySubDraftPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Play a pick in a sub-draft.";
    Description = "Plays a pick scoped to the active sub-draft.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The pick was successfully played.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "Authentication is required to play a pick.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to play a pick in this sub-draft.");
    Response(StatusCodes.Status404NotFound, "The specified sub-draft or pick was not found.");
  }
}
