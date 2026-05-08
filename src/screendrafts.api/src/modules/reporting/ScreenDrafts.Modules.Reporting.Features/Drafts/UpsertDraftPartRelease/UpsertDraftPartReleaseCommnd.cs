namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftPartRelease;

internal sealed record UpsertDraftPartReleaseCommand : ICommand
{
  public Guid DraftId { get; init; }
  public required string DraftPartPublicId { get; init; }
  public required string ReleaseChannel { get; init; }
  public DateOnly ReleaseDate { get; init; }
  public int? EpisodeNumber { get; init; }
}
