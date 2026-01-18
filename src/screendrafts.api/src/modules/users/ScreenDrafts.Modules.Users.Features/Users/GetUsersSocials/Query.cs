
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed record Query : IQuery<Response>
{
  public required List<string> PersonIds { get; init; }
}
