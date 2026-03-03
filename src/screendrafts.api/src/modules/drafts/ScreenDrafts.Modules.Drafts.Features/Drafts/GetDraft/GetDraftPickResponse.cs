namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftPickResponse
{
  public int PlayOrder { get; init; }
  public int Position { get; init; }
  public required string MoviePublicId { get; init; }
  public required string MovieTitle { get; init; }
  public string? MovieVersionName { get; init; }
  public string? ActedByPublicId { get; init; }
  public Guid PlayedByParticipantIdValue { get; init; }
  public int PlayedByParticipantKindValue { get; init; }
  public GetDraftVetoResponse? Veto { get; init; }
  public GetDraftCommissionerOverrideResponse? CommissionerOverride { get; init; }
}
