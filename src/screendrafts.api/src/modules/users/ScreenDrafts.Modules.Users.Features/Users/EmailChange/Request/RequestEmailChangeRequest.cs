namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Request;

internal sealed record RequestEmailChangeRequest
{
  public required string NewEmail { get; init; }
}
