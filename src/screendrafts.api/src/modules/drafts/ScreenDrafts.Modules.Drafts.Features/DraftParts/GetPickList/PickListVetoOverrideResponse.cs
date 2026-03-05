
namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GetPickList;

internal sealed record PickListVetoOverrideResponse
{
  public Guid IssuedByParticipantIdValue { get; init; }
  public int IssuedByParticipantKindValue { get; init; }
  public string? ActedByPublicId { get; init; }
}
