namespace ScreenDrafts.Modules.Drafts.Features.People.GetUsersSocials;

internal sealed record GetUsersSocialsQuery : IQuery<Response>
{
  public required List<string> PersonIds { get; init; }
}
