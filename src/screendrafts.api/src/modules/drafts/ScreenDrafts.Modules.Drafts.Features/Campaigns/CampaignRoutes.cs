namespace ScreenDrafts.Modules.Drafts.Features.Campaigns;

internal static class CampaignRoutes
{
  public const string Campaigns = "/campaigns";
  public const string ById = Campaigns + "/{publicId}";
  public const string Restore = Campaigns + "/{publicId}/restore";
}
