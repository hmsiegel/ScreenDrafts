namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

internal sealed record RemoveHostDraftPartCommand(Guid DraftPartId, Guid HostId) : ICommand;


