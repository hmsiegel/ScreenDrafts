namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AddBoostersChampionAssignment;

internal sealed record AddBoostersChampionAssignmentCommand : ICommand<string>
{
  public required string DraftPartId { get; init; }
  public required string AssignedDrafterPublicId { get; init; }
  public int? TmdbId { get; init; }
}
