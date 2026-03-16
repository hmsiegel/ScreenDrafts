namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.Get;

internal sealed record GetCandidateListQuery : IQuery<GetCandidateListResponse>
{
  public required string DraftPartId { get; init; }
  public required int Page { get; init; }
  public required int PageSize { get; init; }
}
