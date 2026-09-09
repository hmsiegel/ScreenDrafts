namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.UpdateDrafterName;

internal sealed record UpdateDrafterNameCommand : ICommand
{
  public required Guid UserId { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
}
