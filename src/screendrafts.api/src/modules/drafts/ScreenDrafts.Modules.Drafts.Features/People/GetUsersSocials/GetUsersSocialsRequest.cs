namespace ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

internal sealed record GetUsersSocialsRequest
{
  public List<string> PublicIds { get; init; } = [];
}
