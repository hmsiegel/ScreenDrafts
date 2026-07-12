using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Delete;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Soft deletes a draft by its public ID.";
    Description =
      "Soft deletes a draft by its public ID. This operation marks the draft as deleted but does not permanently remove it from the system.";
    Response(StatusCodes.Status400BadRequest, "The request was invalid or cannot be processed.");
    Response(StatusCodes.Status204NoContent, "The draft was successfully soft deleted.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(
      StatusCodes.Status403Forbidden,
      "The user does not have permission to delete the draft."
    );
    Response(StatusCodes.Status404NotFound, "The draft was not found.");
  }
}
