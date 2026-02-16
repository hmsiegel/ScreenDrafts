using FastEndpoints;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Get;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Get;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a series by its public ID.";
    Description = "Retrieves the details of a specific series using its public identifier.";
    Response<SeriesResponse>(StatusCodes.Status200OK, "The series was found and returned successfully.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
  }
}
