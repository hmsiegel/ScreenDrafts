using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class Summary : Summary<Endpoint>

{
  public Summary()
  {
    Summary = "Restores a deleted campaign.";
    Description = "Restores a deleted campaign by its unique identifier.";
    Responses = new()
    {
      { StatusCodes.Status204NoContent, "The campaign was successfully restored." },
      { StatusCodes.Status400BadRequest, "The RestoreCampaignRequest was invalid." },
      { StatusCodes.Status401Unauthorized, "The user is not authenticated." },
      { StatusCodes.Status403Forbidden, "The user does not have permission to restore campaigns." },
      { StatusCodes.Status404NotFound, "The campaign with the specified ID was not found." }
    };
  }
}

