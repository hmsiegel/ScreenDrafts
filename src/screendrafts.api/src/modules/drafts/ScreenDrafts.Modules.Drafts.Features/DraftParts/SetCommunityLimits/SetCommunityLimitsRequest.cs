namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetCommunityLimits;

internal sealed record SetCommunityLimitsRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
  public int MaxCommunityPicks { get; init; }
  public int MaxCommunityVetoes { get; init; }
}
