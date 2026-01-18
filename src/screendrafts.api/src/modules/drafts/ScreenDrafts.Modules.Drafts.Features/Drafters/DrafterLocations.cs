namespace ScreenDrafts.Modules.Drafts.Features.Drafters;

internal static class DrafterLocations
{
    public static string ById(string publicId) => $"/drafters/{publicId}";
}
