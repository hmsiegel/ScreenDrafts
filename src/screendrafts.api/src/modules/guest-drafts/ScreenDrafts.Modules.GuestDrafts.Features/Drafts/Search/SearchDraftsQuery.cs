namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Search;

internal sealed record SearchDraftsQuery : IQuery<PagedResult<GuestDraftSummaryResponse>>
{
  public required string CallerUserPublicId { get; init; }
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 20;

  /// <summary>
  /// Optional filter by GuestDraftStatus name ("Created"/"InProgress"/"Completed").
  /// Not required for the three-section landing page — that fetches everything
  /// once and buckets client-side, same as listAdminActiveDrafts does for
  /// canonical. Included for symmetry with SearchDrafts and any future use
  /// that wants server-side filtering instead.
  /// </summary>
  public string? Status { get; init; }
}
