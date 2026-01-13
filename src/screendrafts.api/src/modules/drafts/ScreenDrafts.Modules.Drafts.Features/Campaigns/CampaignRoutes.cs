namespace ScreenDrafts.Modules.Drafts.Features.Campaigns;

internal static class CampaignRoutes
{
  public const string Campaigns = "/campaigns";
  public const string ById = Campaigns + "/{publicId:string}";
  public const string Restore = Campaigns + "/{publicId:string}/restore";
}
