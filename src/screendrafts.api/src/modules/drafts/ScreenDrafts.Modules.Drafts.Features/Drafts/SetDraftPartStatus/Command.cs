namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand<Response>
{
  public required Request Request { get; init; }
}
