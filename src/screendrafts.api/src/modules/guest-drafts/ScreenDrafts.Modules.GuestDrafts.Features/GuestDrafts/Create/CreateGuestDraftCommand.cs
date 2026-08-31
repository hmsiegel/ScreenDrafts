namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;

internal sealed record CreateGuestDraftCommand : ICommand<string>
{
  public required string OwnerUserPublicId { get; init; }
  public required string Title { get; init; }
  public required string Type { get; init; }
}
