namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetCommunityLimits;

internal sealed record SetCommunityLimitsRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
  public int MaxCommunityPicks { get; init; }
  public int MaxCommunityVetoes { get; init; }
}
