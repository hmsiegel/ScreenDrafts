using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "Get a campaign by its public ID.";
        Description = "Retrieves the details of a specific campaign using its public identifier.";
        Response<CampaignResponse>(StatusCodes.Status200OK, "The campaign was found and returned successfully.");
        Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
        Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    }
}
