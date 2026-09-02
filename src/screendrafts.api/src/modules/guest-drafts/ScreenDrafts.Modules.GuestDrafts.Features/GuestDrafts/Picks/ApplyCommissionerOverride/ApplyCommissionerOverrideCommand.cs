namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyCommissionerOverride;

internal sealed record ApplyCommissionerOverrideCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required int PlayOrder { get; init; }
  public required string CallerUserPublicId { get; init; }
}
