namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Edit;

internal sealed record EditSeriesCommand : ICommand
{
  public required string Name { get; init; }
  public required string PublicId { get; init; }
  public int Kind { get; init; }
  public int CanonicalPolicy { get; init; }
  public int ContinuityScope { get; init; }
  public int ContinuityDateRule { get; init; }
  public int AllowedDraftTypes { get; init; }
  public int? DefaultDraftType { get; init; }
}

