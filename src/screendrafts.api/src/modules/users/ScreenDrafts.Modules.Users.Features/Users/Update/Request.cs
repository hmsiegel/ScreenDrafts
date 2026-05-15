namespace ScreenDrafts.Modules.Users.Features.Users.Update;

public sealed record Request
{
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? MiddleName { get; init; }
}
