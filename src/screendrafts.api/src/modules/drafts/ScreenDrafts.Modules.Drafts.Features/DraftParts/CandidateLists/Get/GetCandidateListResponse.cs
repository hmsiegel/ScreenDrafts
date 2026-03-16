namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.Get;

internal sealed record GetCandidateListResponse
{
  public PagedResult<CandidateListEntryResponse> Response { get; init; } = default!;
}
