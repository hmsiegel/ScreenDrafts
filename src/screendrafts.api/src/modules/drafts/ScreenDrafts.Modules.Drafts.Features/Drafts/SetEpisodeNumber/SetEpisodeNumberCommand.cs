namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetEpisodeNumber;

internal sealed record SetEpisodeNumberCommand : ICommand
{
  public string DraftId { get; init; } = default!;
  public required ReleaseChannel ReleaseChannel { get; init; }
  public int EpisodeNumber { get; init; }
}
