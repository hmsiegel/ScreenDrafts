
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed record Request
{
  public List<string> PublicIds { get; init; } = [];
}
