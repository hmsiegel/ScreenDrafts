namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

internal sealed record GetDraftPartQuery : IQuery<GetDraftPartQueryResponse>
{
  public required string DraftPartId { get; init; }
  public bool IncludePatreon { get; init; }
}
