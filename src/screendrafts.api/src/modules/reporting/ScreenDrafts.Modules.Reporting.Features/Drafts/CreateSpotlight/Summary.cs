using FastEndpoints;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.CreateSpotlight;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new spotlight for a draft.";
    Description =
      "Creates a new draft spotlight, which is a highlighted pick list for a draft. The draft must be complete and not a Patreon draft.";
    Response<CreateSpotlightResponse>(
      StatusCodes.Status201Created,
      "The spotlight was created successfully."
    );
    Response(
      StatusCodes.Status400BadRequest,
      "The request was invalid. This can occur if the draft is incomplete, a spotlight already exists for the draft, or if the draft is a Patreon draft."
    );
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(
      StatusCodes.Status403Forbidden,
      "The user does not have permission to create a spotlight."
    );
    Response(
      StatusCodes.Status404NotFound,
      "The specified draft was not found in the reporting database."
    );
  }
}
