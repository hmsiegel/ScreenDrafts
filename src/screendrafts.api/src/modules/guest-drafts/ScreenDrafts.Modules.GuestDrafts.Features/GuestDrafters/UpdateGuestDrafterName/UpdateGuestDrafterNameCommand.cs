namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.UpdateGuestDrafterName;

internal sealed record UpdateGuestDrafterNameCommand : ICommand
{
  public required Guid UserId { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
}
