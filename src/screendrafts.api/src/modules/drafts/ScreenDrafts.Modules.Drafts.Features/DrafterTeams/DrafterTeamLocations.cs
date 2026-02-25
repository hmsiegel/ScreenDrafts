namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams;

internal static class DrafterTeamLocations
{
  public static string ById(string publicId) => $"{DrafterTeamRoutes.Base}/{publicId}";
}
