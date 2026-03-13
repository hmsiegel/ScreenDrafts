namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.RemoveHost;

internal sealed record RemoveHostDraftPartCommand(Guid DraftPartId, Guid HostId) : ICommand;


