namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ReleaseDates.ListUnreleased;

internal sealed record ListUnreleasedDraftPartsQuery
  : IQuery<PagedResult<UnreleasedDraftPartResponse>>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 50;
  public string? DraftPublicId { get; init; }
}
