namespace ScreenDrafts.Modules.Users.Features.Users.GetByPublicId;

public sealed record GetByPublicIdResponse
{
  public string PublicId { get; init; } = default!;
  public Guid UserId { get; init; }
  public string Email { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string MiddleName { get; init; } = default!;
}
