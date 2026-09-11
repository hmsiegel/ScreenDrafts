namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.CreateDrafter;

internal sealed record CreateDrafterCommand : ICommand<string>
{
  public required Guid UserId { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
}
