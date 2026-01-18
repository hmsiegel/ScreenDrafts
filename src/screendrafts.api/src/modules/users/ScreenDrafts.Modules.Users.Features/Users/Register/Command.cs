namespace ScreenDrafts.Modules.Users.Features.Users.Register;

public sealed record Command : Common.Features.Abstractions.Messaging.ICommand<string>
{
  public string Email { get; init; } = default!;
  public string Password { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? MiddleName { get; init; }
}
