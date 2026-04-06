using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AdvanceSubDraft;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Advance to the next sub-draft.";
    Description = "Completes the current sub-draft, carries unused vetoes forward to the next sub-draft, and initializes participant veto counts.";
    Response(StatusCodes.Status204NoContent, "Sub-draft advanced successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Draft part or sub-draft not found.");
  }
}
