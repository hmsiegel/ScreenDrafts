namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;

internal sealed record CreateGuestDraftRequest
{
  public required string Title { get; init; }
  public required string Type { get; init; }
}
