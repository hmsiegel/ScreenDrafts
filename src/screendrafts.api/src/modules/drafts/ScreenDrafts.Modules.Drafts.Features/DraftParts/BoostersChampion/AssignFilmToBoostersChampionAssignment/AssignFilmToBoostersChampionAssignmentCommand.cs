namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AssignFilmToBoostersChampionAssignment;

internal sealed record AssignFilmToBoostersChampionAssignmentCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string AssignmentPublicId { get; init; }
  public required int TmdbId { get; init; }
}
