
namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed record DrafterListItem
{
  public required string DrafterId { get; init; }
  public required string PersonId { get; init; }
  public required string DisplayName { get; init; }
  public required bool IsRetired { get; init; }
}
