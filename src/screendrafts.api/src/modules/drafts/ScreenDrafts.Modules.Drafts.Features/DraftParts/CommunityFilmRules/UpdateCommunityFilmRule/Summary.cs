using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.UpdateCommunityFilmRule;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Update a community film rule in a draft.";
    Description =
      "This endpoint allows you to update a community film rule in a specific draft. A community film rule is a rule for the community picks and vetoes in the draft.";
    Response(StatusCodes.Status204NoContent, "The community film rule was updated successfully.");
    Response(
      StatusCodes.Status400BadRequest,
      "The request was invalid. This can occur if the draft ID is not valid, the community film rule data is invalid, or if the user does not have permission to update a community film rule in the draft."
    );
    Response(
      StatusCodes.Status401Unauthorized,
      "The user is not authenticated. This can occur if the user is not logged in or if the authentication token is missing or invalid."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "The user does not have permission to update a community film rule in the draft."
    );
    Response(StatusCodes.Status404NotFound, "The specified draft was not found.");
  }
}
