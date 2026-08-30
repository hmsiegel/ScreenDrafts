using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.GetSeriesMetadata;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get series metadata.";
    Description =
      "Returns the valid values for series policies and draft type options for UI picklists.";
    Response<GetSeriesMetadataResponse>(
      StatusCodes.Status200OK,
      "Returns the series metadata.",
      contentType: "application/json"
    );
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
