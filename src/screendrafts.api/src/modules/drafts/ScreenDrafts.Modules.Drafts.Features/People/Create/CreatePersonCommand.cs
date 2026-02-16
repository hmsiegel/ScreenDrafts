namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed record CreatePersonCommand : ICommand<string>
{
  public Guid? UserId { get; init; }
  public string PublicId { get; init; } = string.Empty;
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
}

