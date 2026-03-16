namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.Get;

internal sealed record GetCandidateListRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromQuery]
  public int Page { get; init; } = 1;

  [FromQuery]
  public int PageSize { get; init; } = 50;
}
