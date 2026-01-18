namespace ScreenDrafts.Modules.Users.PublicApi;

public sealed record UserResponse
{
  public Guid UserId { get; init; }
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? MiddleName { get; init; } = default!;
}
