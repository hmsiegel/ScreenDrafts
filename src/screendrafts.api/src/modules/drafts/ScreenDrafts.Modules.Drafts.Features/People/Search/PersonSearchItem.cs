namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record PersonSearchItem
{
  public string PublicId { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
}
