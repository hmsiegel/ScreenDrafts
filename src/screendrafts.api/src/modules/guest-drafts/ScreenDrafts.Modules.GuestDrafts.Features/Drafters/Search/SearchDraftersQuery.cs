namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.Search;

internal sealed record SearchDraftersQuery : IQuery<IReadOnlyList<GuestDrafterSummaryResponse>>
{
  public string? Search { get; init; }
}
