namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

public sealed record ListDraftsReleaseResponse
{
  public int ReleaseChannel { get; init; }
  public int? EpisodeNumber { get; init; }
  public DateOnly ReleaseDate { get; init; }
}
