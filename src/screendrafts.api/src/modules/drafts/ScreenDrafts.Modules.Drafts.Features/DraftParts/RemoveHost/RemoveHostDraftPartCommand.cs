namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

public sealed record RemoveHostDraftPartCommand(Guid DraftPartId, Guid HostId) : ICommand;


