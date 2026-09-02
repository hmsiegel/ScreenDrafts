namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVeto;

internal sealed record ApplyVetoCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required int PlayOrder { get; init; }
  public required string CallerUserPublicId { get; init; }
  public string? Note { get; init; }
}
