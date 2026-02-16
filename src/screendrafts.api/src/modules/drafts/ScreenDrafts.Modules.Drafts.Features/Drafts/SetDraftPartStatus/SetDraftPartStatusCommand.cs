namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed record SetDraftPartStatusCommand : ICommand<Response>
{
  public required SetDraftPartStatusRequest SetDraftPartStatusRequest { get; init; }
}


