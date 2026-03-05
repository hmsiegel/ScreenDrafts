
namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GetPickList;

internal sealed record PickListVetoResponse
{
  public Guid IssuedByParticipantIdValue { get; init; }
  public int IssuedByParticipantKindValue { get; init; }
  public string? ActedByPublicId { get; init; }
  public string? Note { get; init; }
  public DateTime OccurredOntUtc { get; init; }
  public bool IsOverridden { get; init; }
  public PickListVetoOverrideResponse? Override { get; init; }
}
