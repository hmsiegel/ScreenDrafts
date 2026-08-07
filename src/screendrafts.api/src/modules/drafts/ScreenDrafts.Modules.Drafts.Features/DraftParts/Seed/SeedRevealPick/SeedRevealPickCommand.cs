namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedRevealPick;

internal sealed record SeedRevealPickCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required int PlayOrder { get; init; }
  public required string ActedByPublicId { get; init; }
}
