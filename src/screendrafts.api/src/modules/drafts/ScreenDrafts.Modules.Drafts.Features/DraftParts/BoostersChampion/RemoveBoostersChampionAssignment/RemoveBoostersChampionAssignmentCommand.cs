namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.RemoveBoostersChampionAssignment;

internal sealed record RemoveBoostersChampionAssignmentCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string AssignmentPublicId { get; init; }
}
