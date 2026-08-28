namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayBoostersChampionAssignmentResponse
{
  public required string PublicId { get; init; }
  public required string AssignedDrafterPublicId { get; init; }
  public required string AssignedDrafterDisplayName { get; init; }
  public int? TmdbId { get; init; }
  public string? Title { get; init; }
}
