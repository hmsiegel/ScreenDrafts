namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Search;

internal sealed record GuestDraftSummaryResponse
{
  public required string PublicId { get; init; }
  public required string Title { get; init; }
  public required string Type { get; init; }
  public required string Status { get; init; }

  public DateOnly? DraftDate { get; init; }

  /// <summary>
  /// True when the caller is this draft's owner. False for a caller who's
  /// merely a participant (or, in principle, neither — though the query's
  /// WHERE clause never returns a row for that case). Frontend uses this to
  /// gate Setup/Start links to owners only, mirroring the backend's own
  /// OnlyOwnerCanPerformThisAction checks.
  /// </summary>
  public bool IsOwner { get; init; }
}
