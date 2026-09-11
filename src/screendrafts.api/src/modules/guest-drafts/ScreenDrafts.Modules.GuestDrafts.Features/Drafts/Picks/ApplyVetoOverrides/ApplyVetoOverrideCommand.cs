namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVetoOverrides;

internal sealed record ApplyVetoOverrideCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required int PlayOrder { get; init; }
  public required string CallerUserPublicId { get; init; }
  public string? Note { get; init; }
}
