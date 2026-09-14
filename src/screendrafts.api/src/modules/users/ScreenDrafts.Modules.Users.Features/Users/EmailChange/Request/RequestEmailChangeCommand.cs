namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Request;

internal sealed record RequestEmailChangeCommand : ICommand
{
  public required string PublicId { get; init; }
  public required string NewEmail { get; init; }
}
