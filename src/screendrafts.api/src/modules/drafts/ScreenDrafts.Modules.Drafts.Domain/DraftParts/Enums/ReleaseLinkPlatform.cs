namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;

/// <summary>
/// A podcast platform a released episode is linked on.
/// </summary>
/// <param name="name">The name.</param>
/// <param name="value">The value.</param>
public sealed class ReleaseLinkPlatform(string name, int value)
  : SmartEnum<ReleaseLinkPlatform>(name, value)
{
  public static readonly ReleaseLinkPlatform Spotify = new(nameof(Spotify), 0);
  public static readonly ReleaseLinkPlatform Apple = new(nameof(Apple), 1);
}
