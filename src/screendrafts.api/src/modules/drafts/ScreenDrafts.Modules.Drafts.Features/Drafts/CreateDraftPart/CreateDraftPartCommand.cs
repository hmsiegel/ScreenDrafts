namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed record CreateDraftPartCommand : ICommand<string>
{
  public string DraftId { get; set; } = default!;
  public int PartIndex { get; set; } 
  public int MinimumPosition { get; set; } 
  public int MaximumPosition { get; set; }
}
