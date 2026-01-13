namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed record Command(string PublicId) : Common.Features.Abstractions.Messaging.ICommand;
