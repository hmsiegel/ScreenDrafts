namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set Campaign for Draft";
    Description = "Sets the campaign for a specific draft.";
    Response(StatusCodes.Status204NoContent, "The campaign was successfully set for the draft.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to modify this draft.");
  }
}
