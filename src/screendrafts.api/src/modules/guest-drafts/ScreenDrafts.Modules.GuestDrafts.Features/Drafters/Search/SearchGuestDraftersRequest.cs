namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.Search;

internal sealed record SearchGuestDraftersRequest
{
  public string? Search { get; init; }
}
