namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategory;

internal sealed record SetCategoryDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; init; } = default!;

  [FromBody]
  public string CategoryId { get; init; } = default!;
}
