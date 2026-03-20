namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed record SetDraftPartStatusCommand : ICommand<Response>
{
  public required string DraftPublicId { get; init; }
  public required int PartIndex { get; init; }
  public DraftPartStatusAction Action { get; init; }
}


