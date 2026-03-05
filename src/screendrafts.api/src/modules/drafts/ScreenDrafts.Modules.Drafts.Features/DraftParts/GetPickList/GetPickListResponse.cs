
namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GetPickList;

internal sealed record GetPickListResponse
{
  public IReadOnlyList<PickListItemResponse> Picks { get; init; } = [];
}
