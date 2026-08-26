namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftBoostersChampionAssignmentResponse
{
  public required string PublicId { get; init; }
  public required string AssignedDrafterPublicId { get; init; }
  public required string AssignedDrafterDisplayName { get; init; }
  public int? TmdbId { get; init; }
  public string? Title { get; init; }
}
