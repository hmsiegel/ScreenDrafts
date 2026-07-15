namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Restore;

internal sealed record RestoreSeriesCommand : ICommand
{
  public required string PublicId { get; init; }
}
