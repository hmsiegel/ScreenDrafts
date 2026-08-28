namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AssignFilmToBoostersChampionAssignment;

internal sealed record AssignFilmToBoostersChampionAssignmentRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "assignmentPublicId")]
  public string AssignmentPublicId { get; init; } = default!;

  public required int TmdbId { get; init; }
}
