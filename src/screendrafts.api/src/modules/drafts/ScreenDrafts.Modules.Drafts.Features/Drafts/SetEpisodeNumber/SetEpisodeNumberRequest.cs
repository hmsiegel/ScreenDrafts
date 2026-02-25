namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetEpisodeNumber;

internal sealed record SetEpisodeNumberRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; init; } = default!;
  public int ReleaseChannel { get; init; } = default!;
  public int EpisodeNumber { get; init; }
}
