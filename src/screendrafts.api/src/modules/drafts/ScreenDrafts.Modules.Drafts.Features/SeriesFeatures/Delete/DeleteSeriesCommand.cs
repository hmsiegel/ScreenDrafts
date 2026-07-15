namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Delete;

internal sealed record DeleteSeriesCommand : ICommand
{
  public required string PublicId { get; init; }
}
