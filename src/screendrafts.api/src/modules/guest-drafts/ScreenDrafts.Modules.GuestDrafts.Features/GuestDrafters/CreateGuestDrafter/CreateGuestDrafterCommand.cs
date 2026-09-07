namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.CreateGuestDrafter;

internal sealed record CreateGuestDrafterCommand : ICommand<string>
{
  public required Guid UserId { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
}
