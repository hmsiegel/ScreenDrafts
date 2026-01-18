
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed record Response
{
  public List<SocialResponse> Socials { get; init; } = [];
}
