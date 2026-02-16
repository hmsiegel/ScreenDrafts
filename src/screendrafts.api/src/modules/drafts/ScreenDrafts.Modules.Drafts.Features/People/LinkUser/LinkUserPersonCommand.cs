
namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed record LinkUserPersonCommand : ICommand
{
  public required string PublicId { get; init; }
  public required Guid UserId { get; init; }
}

