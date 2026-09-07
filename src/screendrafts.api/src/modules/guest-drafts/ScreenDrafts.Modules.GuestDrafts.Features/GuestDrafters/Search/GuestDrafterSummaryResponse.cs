namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.Search;

internal sealed record GuestDrafterSummaryResponse
{
  public required string PublicId { get; init; }
  public required string DisplayName { get; init; }
}
