namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.Search;

internal sealed record GuestDrafterSummaryResponse
{
  public required string PublicId { get; init; }
  public required string DisplayName { get; init; }
}
