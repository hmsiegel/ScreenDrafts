namespace ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

internal sealed record Request
{
  public List<string> PublicIds { get; init; } = [];
}
