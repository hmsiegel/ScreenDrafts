
namespace ScreenDrafts.Modules.Users.Features.Users.UpdatePerson;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand
{
  public Guid UserId { get; init; }
  public Guid PersonId { get; init; }
  public string PersonPublicId { get; init; } = string.Empty;
}
