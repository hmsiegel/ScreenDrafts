namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftPosition;

internal sealed record AssignSubDraftPositionRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "subDraftId")]
  public string SubDraftId { get; init; } = default!;
  public string WinnerParticipantPublicId { get; init; } = default!;
  public int WinnerParticipantKind { get; init; }
  public string Choice { get; init; } = default!;
}
