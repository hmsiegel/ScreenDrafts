using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set the subject of a sub-draft.";
    Description = "Sets the subject of a specific sub-draft within a draft after it is revealed.";
    Response(StatusCodes.Status204NoContent, "The subject of the sub-draft was successfully set.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. This can occur if the draft or sub-draft does not exist, if the sub-draft is not revealed, or if the provided subject is invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to set the subject of this sub-draft.");
    Response(StatusCodes.Status404NotFound, "The specified draft or sub-draft was not found.");
  }
}
