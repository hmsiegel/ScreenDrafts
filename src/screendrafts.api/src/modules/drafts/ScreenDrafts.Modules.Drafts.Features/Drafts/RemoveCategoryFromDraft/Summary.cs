using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Removes a category from a draft.";
    Description = "This endpoint allows you to remove a specific category from a draft by providing the draft ID and category ID.";
    Response(StatusCodes.Status204NoContent, "The category was successfully removed from the draft.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access. Authentication is required.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access. You do not have permission to remove categories from this draft.");
    Response(StatusCodes.Status404NotFound, "The specified draft or category was not found.");
  }
}
