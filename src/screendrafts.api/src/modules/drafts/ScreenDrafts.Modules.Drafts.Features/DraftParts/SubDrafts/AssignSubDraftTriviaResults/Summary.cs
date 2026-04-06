using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftTriviaResults;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
     Summary = "Assigns trivia results to a sub-draft.";
    Description = "Assigns one-question trivia results to a sub-draft and activates it for picking.";
    Response(StatusCodes.Status204NoContent, "Trivia results assigned and sub-draft activated.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Draft part or sub-draft not found.");
  }
}
