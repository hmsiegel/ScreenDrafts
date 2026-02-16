namespace ScreenDrafts.Modules.Drafts.Features.People.Edit;

internal sealed record EditPersonCommand : ICommand
{
  public string PublicId { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? DisplayName { get; init; }
}

