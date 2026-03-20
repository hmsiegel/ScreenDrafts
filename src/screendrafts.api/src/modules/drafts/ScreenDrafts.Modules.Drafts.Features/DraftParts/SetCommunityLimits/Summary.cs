using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetCommunityLimits;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set the community picks and veto limits for a draft part.";
    Description = "Administrator-only. Sets the maximum number of community picks and vetoes " +
      "allowed for the specified draft part. Both values must be zero or greater.";
    Response(StatusCodes.Status204NoContent, "Limits successfully updated.");
    Response(StatusCodes.Status400BadRequest, "Invalid request. Check the error message for details.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized. Authentication is required.");
    Response(StatusCodes.Status403Forbidden, "Forbidden. You do not have permission to perform this action.");
    Response(StatusCodes.Status404NotFound, "Draft part not found.");
  }
}
