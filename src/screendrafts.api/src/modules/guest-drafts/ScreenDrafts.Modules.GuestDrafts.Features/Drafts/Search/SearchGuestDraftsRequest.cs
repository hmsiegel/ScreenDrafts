namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Search;

internal sealed record SearchGuestDraftsRequest
{
  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 20;

  [FromQuery(Name = "status")]
  public string? Status { get; init; }
}
