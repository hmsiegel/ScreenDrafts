namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AddBoostersChampionAssignment;

internal sealed record AddBoostersChampionAssignmentRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  public required string AssignedDrafterPublicId { get; init; }
  public int? TmdbId { get; init; }
}
