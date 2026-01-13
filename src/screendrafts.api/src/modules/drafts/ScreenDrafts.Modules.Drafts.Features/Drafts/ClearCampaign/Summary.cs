namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Clears the current campaign from a draft.";
    Description = "Clears the current campaign from a draft.";
    Response(StatusCodes.Status204NoContent, "Campaign removed from draft successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
  }
}
