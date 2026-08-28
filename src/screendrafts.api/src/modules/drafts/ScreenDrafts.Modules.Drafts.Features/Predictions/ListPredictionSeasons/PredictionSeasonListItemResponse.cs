namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed record PredictionSeasonListItemResponse
{
  private readonly List<PredictionSeasonDraftResponse> _drafts = [];

  public string PublicId { get; init; } = default!;
  public int Number { get; init; }
  public DateOnly StartsOn { get; init; }
  public DateOnly? EndsOn { get; init; }
  public int TargetPoints { get; init; }
  public bool IsClosed { get; init; }

  public IReadOnlyList<PredictionSeasonDraftResponse> Drafts => _drafts.AsReadOnly();

  public void SetDrafts(IReadOnlyList<PredictionSeasonDraftResponse> drafts)
  {
    _drafts.Clear();
    _drafts.AddRange(drafts);
  }
}
