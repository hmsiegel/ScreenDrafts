using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Assign trivia results to a draft part.";
    Description = "Assigns the trivia results to a draft part, which is used to determine the order of the draft positions.";
    Response(StatusCodes.Status204NoContent, "Trivia results assigned successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Draft part not found.");
  }
}
