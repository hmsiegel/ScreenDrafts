namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.RevealPick;

internal sealed record RevealPickCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required int PlayOrder { get; init; }
  public required string CallerUserPublicId { get; init; }
}
