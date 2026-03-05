namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.GetPickList;

internal sealed record GetPickListResponse
{
  public IReadOnlyList<PickListItemResponse> Picks { get; init; } = [];
}
