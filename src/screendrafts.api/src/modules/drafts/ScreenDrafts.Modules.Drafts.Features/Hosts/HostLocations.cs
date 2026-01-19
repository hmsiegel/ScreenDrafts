namespace ScreenDrafts.Modules.Drafts.Features.Hosts;

internal static class HostLocations
{
  public static string ById(string publicId) => $"/hosts/{publicId}";
}
