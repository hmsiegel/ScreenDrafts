namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateProfile;

internal sealed record UpdateProfileCommand : ICommand
{
  public required string PublicId { get; init; }
  public string? DisplayName { get; init; }
  public string? Biography { get; init; }
  public string? Location { get; init; }
}
