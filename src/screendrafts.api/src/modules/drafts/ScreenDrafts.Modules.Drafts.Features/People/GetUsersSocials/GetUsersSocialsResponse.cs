namespace ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

internal sealed record GetUsersSocialsResponse
{
  public List<SocialResponse> Socials { get; init; } = [];
}
