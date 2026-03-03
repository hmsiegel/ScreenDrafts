namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftQuery : IQuery<GetDraftResponse>
{
  public required string DraftId { get; set; }
  public bool IncludePatreon { get; set; }
}
