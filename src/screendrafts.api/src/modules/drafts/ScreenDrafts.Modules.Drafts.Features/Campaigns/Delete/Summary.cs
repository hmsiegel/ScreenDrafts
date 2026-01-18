using ScreenDrafts.Modules.Drafts.Features.Categories.Delete;
using ScreenDrafts.Modules.Drafts.Features.Series.Delete;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Delete a campaign by its public ID.";
    Description = "Deletes a campaign identified by its public ID. Requires appropriate permissions.";
    Response(StatusCodes.Status204NoContent, "Campaign deleted successfully.");
    Response(StatusCodes.Status404NotFound, "Campaign not found.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
  }
}
