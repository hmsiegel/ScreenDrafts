using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Restore;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Restores a series that was previously soft deleted.";
    Description = "Restores a series that was previously soft deleted.";
    Response(StatusCodes.Status204NoContent, "Series restored successfully.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized to restore series.");
    Response(StatusCodes.Status403Forbidden, "Forbidden to restore series.");
    Response(StatusCodes.Status404NotFound, "Series not found.");
    Response(StatusCodes.Status409Conflict, "Conflict occurred while restoring series.");
  }
}
