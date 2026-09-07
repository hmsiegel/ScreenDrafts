namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.Search;

internal sealed record SearchGuestDraftersQuery : IQuery<IReadOnlyList<GuestDrafterSummaryResponse>>
{
  public string? Search { get; init; }
}
