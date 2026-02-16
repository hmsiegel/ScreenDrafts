namespace ScreenDrafts.Modules.Users.Features.Users.GetByUserId;

public sealed record GetByUserIdResponse
{
  public Guid UserId { get; init; }
  public string PublicId { get; init; } = default!;
  public string Email { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string MiddleName { get; init; } = default!;
}

