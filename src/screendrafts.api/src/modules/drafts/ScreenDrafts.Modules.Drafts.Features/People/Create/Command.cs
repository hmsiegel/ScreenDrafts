namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand<string>
{
  public Guid? UserId { get; init; }
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
}
