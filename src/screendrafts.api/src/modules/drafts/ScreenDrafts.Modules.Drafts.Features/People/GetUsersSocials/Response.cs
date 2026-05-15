using ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

namespace ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

internal sealed record Response
{
  public List<SocialResponse> Socials { get; init; } = [];
}
