namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed class Summary : ScreenDraftsSummary<Endpoint, EditCampaignRequest>
{
  public Summary()
  {
    Summary = "Edits an existing campaign.";
    Description = "Edits an existing campaign.";
    ExampleRequest = new EditCampaignRequest
    {
      Name = "New Campaign Name",
      Slug = "new-campaign-slug"
    };
    Response(StatusCodes.Status204NoContent, "Campaign edited successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid EditCampaignRequest.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Campaign not found.");
  }
}

