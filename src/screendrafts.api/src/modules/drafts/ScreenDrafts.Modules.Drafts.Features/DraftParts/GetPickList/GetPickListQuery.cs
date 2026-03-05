
namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GetPickList;

internal sealed record GetPickListQuery : IQuery<GetPickListResponse>
{
  public required string DraftPartId { get; init; }
}
