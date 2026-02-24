namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

internal sealed record SetCategoriesDraftCommand : ICommand
{
  [FromRoute(Name = "draftId")]
  public string DraftId { get; set; } = default!;
  public List<string> CategoryIds { get; set; } = [];
}
