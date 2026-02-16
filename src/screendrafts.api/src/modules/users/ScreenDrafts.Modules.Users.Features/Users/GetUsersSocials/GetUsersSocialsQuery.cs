
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed record GetUsersSocialsQuery : IQuery<Response>
{
  public required List<string> PersonIds { get; init; }
}
