namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed record RevealPickCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required int PlayOrder { get; init; }
  public required string ActorPublicId { get; init; }
}
