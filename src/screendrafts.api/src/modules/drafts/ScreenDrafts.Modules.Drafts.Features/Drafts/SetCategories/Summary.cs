using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Sets the categories for a draft.";
    Description = "Sets the categories for a draft.";
    Response(StatusCodes.Status204NoContent);
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
  }
}
