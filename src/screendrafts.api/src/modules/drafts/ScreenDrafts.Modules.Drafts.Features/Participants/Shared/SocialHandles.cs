namespace ScreenDrafts.Modules.Drafts.Features.Participants.Shared;

internal sealed record SocialHandles
{
  public string? Twitter { get; init; }
  public string? Instagram { get; init; }
  public string? Letterboxd { get; init; }
  public string? Bluesky { get; init; }
  public string? ProfilePicturePath { get; init; }
}
