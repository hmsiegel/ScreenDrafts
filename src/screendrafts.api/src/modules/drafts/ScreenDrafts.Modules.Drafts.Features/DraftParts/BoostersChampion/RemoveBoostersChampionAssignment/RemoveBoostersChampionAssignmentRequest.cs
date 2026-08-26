namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.RemoveBoostersChampionAssignment;

internal sealed record RemoveBoostersChampionAssignmentRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "assignmentPublicId")]
  public string AssignmentPublicId { get; init; } = default!;
}
