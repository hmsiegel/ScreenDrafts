using FastEndpoints;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Metadata;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Metadata;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get series metadata.";
    Description = "Returns the valid values for series policies and draft type options for UI picklists.";
    Response<Response>(StatusCodes.Status200OK, "Returns the series metadata.", contentType: "application/json");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
