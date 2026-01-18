namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed record Request
{
  public bool IncludeDeleted { get; init; }
}
