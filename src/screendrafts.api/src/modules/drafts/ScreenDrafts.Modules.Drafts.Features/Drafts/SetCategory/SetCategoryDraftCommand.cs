namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategory;

internal sealed record SetCategoryDraftCommand : ICommand
{
  public string DraftId { get; set; } = default!;
  public string CategoryId { get; set; } = default!;
}
