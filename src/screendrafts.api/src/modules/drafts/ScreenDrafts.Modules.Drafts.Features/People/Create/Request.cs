namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed record Request
{
  public Guid? UserId { get; init; }
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
}
