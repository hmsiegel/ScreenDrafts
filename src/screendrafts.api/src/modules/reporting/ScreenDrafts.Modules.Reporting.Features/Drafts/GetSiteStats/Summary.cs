using FastEndpoints;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetSiteStats;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get home page statistics";
    Description = "Get the statistics for the home page.";
    Response<GetSiteStatsResponse>(StatusCodes.Status200OK, "The statistics for the home page.");
  }
}
