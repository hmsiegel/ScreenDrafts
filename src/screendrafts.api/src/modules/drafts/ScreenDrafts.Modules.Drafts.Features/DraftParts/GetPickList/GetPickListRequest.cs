
namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GetPickList;

internal sealed record GetPickListRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
