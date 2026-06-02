namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftCategoryResponse
{
  public required string PublicId { get; init; }
  public required string Name { get; init; }
}
