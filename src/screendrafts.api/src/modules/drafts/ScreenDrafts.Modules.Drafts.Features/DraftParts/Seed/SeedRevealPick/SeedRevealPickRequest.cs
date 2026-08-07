namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedRevealPick;

internal sealed record SeedRevealPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }

  public required string ActedByPublicId { get; init; }
}
