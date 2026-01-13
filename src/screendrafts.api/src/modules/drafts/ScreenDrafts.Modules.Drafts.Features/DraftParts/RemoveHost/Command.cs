namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

public sealed record Command(Guid DraftPartId, Guid HostId) : Common.Features.Abstractions.Messaging.ICommand;

