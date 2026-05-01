namespace ScreenDrafts.Modules.Users.Features.Users.RegisterSocialUser;

internal sealed record RegisterSocialUserCommand : ICommand<string>
{
  public required string Email { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
  public required string IdentityId { get; init; }
}
