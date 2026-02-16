namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

internal sealed record CreateSeriesCommand : ICommand<Guid>
{
  public required string Name { get; init; }
  public int Kind { get; init; }
  public int CanonicalPolicy { get; init; }
  public int ContinuityScope { get; init; }
  public int ContinuityDateRule { get; init; }
  public int AllowedDraftTypes { get; init; }
  public int? DefaultDraftType { get; init; }
}
