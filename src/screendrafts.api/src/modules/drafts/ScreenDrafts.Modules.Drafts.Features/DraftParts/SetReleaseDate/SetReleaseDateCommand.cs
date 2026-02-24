namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

internal sealed record SetReleaseDateCommand : ICommand
{
  public string DraftPartId { get; set; } = default!;
  public DateOnly ReleaseDate { get; set; }
  public ReleaseChannel ReleaseChannel { get; set; } = ReleaseChannel.MainFeed;
}
