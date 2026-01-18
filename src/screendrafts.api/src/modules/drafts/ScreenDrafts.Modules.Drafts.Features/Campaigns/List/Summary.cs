using ScreenDrafts.Modules.Drafts.Features.Series.List;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Lists all campaigns.";
    Description = "Returns a reference list of campaigns, sorted by name. Administrators may pass an optional query parameter to include deleted campaigns.";
    Response<CampaignCollectionResponse>(StatusCodes.Status200OK, "The list of campaigns.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
