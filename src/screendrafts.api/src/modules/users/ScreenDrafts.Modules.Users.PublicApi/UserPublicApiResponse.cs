namespace ScreenDrafts.Modules.Users.PublicApi;

public sealed record UserPublicApiResponse
{
  public Guid UserId { get; init; }
  public string PublicId { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? MiddleName { get; init; } = default!;
  public string Email { get; init; } = default!;
  public string IdentityId { get; init; } = default!;
}
