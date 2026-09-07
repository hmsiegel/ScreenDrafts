namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;

internal sealed record PlayPickCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string MoviePublicId { get; init; }
  public required int Position { get; init; }
  public required int PlayOrder { get; init; }
  public required string CallerUserPublicId { get; init; }
}
