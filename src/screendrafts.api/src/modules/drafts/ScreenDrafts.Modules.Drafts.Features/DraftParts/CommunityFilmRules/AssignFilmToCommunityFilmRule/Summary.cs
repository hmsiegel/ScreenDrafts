using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AssignFilmToCommunityFilmRule;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Assigns a film to a community film rule in a draft.";
    Description =
      "Assigns a film to a community film rule in a draft. This operation is idempotent.";
    Response(
      StatusCodes.Status204NoContent,
      "The film was successfully assigned to the community film rule."
    );
    Response(
      StatusCodes.Status400BadRequest,
      "The request was invalid. This can occur if the draft part ID, rule ID, or TMDB ID is invalid."
    );
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(
      StatusCodes.Status403Forbidden,
      "The user does not have permission to assign a film to a community film rule in this draft."
    );
    Response(StatusCodes.Status404NotFound, "The draft part or community film rule was not found.");
  }
}
