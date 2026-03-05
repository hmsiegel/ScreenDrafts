namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.GetPickList;

internal sealed record GetPickListQuery : IQuery<GetPickListResponse>
{
  public required string DraftPartId { get; init; }
}
