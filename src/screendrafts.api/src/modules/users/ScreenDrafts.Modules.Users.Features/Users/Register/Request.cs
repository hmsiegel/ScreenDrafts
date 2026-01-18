namespace ScreenDrafts.Modules.Users.Features.Users.Register;

internal sealed record Request
{
  public string Email { get; init; } = default!;
  public string Password { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? MiddleName { get; init; }
}
