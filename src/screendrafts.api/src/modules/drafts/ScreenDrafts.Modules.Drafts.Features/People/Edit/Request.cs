namespace ScreenDrafts.Modules.Drafts.Features.People.Edit;

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? DisplayName { get; init; }
}
