namespace ScreenDrafts.Modules.Drafts.Features.Campaigns;

internal static class CampaignLocations
{
  public static string ById(string publicId) => $"/campaigns/{publicId}";
}
