namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.GetPickList;

internal sealed record PickListItemResponse
{
  public int PlayOrder { get; init; }
  public int Position { get; init; }
  public string MovieImdbId { get; init; } = default!;
  public string MovieTitle { get; init; } = default!;
  public string? MovieVersionName { get; init; } = default!;
  public Guid PlayedByParticipantIdValue { get; init; }
  public int PlayedByParticipantKindValue { get; init; }
  public string? ActedByPublicId { get; init; }
  public PickListVetoResponse? Veto { get; init; }
  public bool HasCommissionerOverride { get; init; }
}
