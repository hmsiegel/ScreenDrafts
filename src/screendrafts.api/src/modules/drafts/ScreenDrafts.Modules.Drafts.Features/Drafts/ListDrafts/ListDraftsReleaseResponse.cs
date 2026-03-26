namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

public sealed record ListDraftsReleaseResponse
{
  public ReleaseChannel ReleaseChannel { get; init; } = default!;
  public int? EpisodeNumber { get; init; }
  public DateOnly ReleaseDate { get; init; }
}
