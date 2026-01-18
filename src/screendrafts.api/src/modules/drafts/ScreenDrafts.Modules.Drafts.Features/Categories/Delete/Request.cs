namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed record Request
{
  public required string PublicId { get; init; }
}
